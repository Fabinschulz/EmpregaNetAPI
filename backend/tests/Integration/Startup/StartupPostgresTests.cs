using System.Diagnostics;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Startup;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace EmpregaNet.Tests.Integration.Startup;

/// <summary>
/// Inicialização contra um PostgreSQL real: migrações de verdade (inclusive <c>CREATE EXTENSION</c>), seed,
/// key ring e a corrida entre réplicas. Requer <c>EMPREGANET_TEST_PG</c> (veja <see cref="PostgresFactAttribute"/>).
/// </summary>
public sealed class StartupPostgresTests
{
    private static readonly Dictionary<string, string?> FastStartup = new()
    {
        ["Seed:AdminEmail"] = "admin@teste.local",
        ["Seed:AdminUserName"] = "admin",
        ["Seed:AdminPassword"] = "Admin@12345",
        ["Startup:DatabaseWaitTimeoutSeconds"] = "5",
        ["Startup:DatabaseWaitMaxDelaySeconds"] = "1",
        ["Startup:LockTimeoutSeconds"] = "60"
    };

    private static ReplicaHost NewReplica(string connectionString, params (string Key, string? Value)[] extra)
    {
        var settings = new Dictionary<string, string?>(FastStartup);
        foreach (var (key, value) in extra)
        {
            settings[key] = value;
        }

        return ReplicaHost.Create(
            new ReplicaDatabase.Postgres(connectionString), settings, environment: "Staging", withStartup: true);
    }

    private static async Task<StartupSnapshot> SnapshotAsync(string connectionString)
    {
        var options = new DbContextOptionsBuilder<PostgreSqlContext>().UseNpgsql(connectionString).Options;
        await using var context = new PostgresContextForAssertions(options);

        var roleNames = await context.Roles.Select(r => r.Name!).ToListAsync();
        var adminRoleId = await context.Roles.Where(r => r.Name == "Admin").Select(r => r.Id).SingleAsync();
        var adminClaims = await context.RoleClaims.Where(c => c.RoleId == adminRoleId).Select(c => c.ClaimValue!).ToListAsync();
        var admins = await context.Users.CountAsync(u => u.Email == "admin@teste.local");
        var keys = await context.DataProtectionKeys.CountAsync();
        var history = await context.Database.SqlQueryRaw<string>("SELECT \"MigrationId\" AS \"Value\" FROM \"__EFMigrationsHistory\"").ToListAsync();
        var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();

        return new StartupSnapshot(roleNames, adminClaims, admins, keys, history, pending);
    }

    private sealed record StartupSnapshot(
        List<string> RoleNames, List<string> AdminClaims, int Admins, int Keys, List<string> MigrationHistory, List<string> Pending);

    private sealed class PostgresContextForAssertions(DbContextOptions<PostgreSqlContext> options) : PostgreSqlContext(options);

    [PostgresFact]
    public async Task Banco_vazio_recebe_migracoes_extensoes_seed_e_key_ring()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        using var replica = NewReplica(database.ConnectionString);

        await replica.Coordinator.RunAsync();

        var snapshot = await SnapshotAsync(database.ConnectionString);
        snapshot.Pending.Should().BeEmpty();
        snapshot.RoleNames.Should().BeEquivalentTo("Admin", "Candidate", "Recruiter", "Manager");
        snapshot.Admins.Should().Be(1);
        snapshot.Keys.Should().Be(1);

        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT count(*) FROM pg_extension WHERE extname IN ('unaccent', 'pg_trgm')", connection);
        ((long)(await command.ExecuteScalarAsync())!).Should().Be(2, "as migrações criam as extensões que o RDS precisa suportar");
    }

    [PostgresFact]
    public async Task Quatro_replicas_subindo_ao_mesmo_tempo_nao_geram_conflito_nem_duplicidade()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var replicas = Enumerable.Range(0, 4).Select(_ => NewReplica(database.ConnectionString)).ToList();

        try
        {
            var results = await Task.WhenAll(replicas.Select(async replica =>
            {
                try
                {
                    await replica.Coordinator.RunAsync();
                    return (Exception?)null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }));

            results.Should().OnlyContain(r => r == null, "todas as réplicas terminam sem erro: %s", string.Join(" | ", results.Where(r => r != null).Select(r => r!.Message)));
        }
        finally
        {
            replicas.ForEach(r => r.Dispose());
        }

        var snapshot = await SnapshotAsync(database.ConnectionString);
        snapshot.RoleNames.Should().BeEquivalentTo("Admin", "Candidate", "Recruiter", "Manager");
        snapshot.Admins.Should().Be(1);
        snapshot.Keys.Should().Be(1, "o key ring é criado uma única vez");
        snapshot.AdminClaims.Should().OnlyHaveUniqueItems("o seed não pode duplicar permissões da role Admin");
        snapshot.MigrationHistory.Should().OnlyHaveUniqueItems();
        snapshot.Pending.Should().BeEmpty();
    }

    [PostgresFact]
    public async Task Executar_a_inicializacao_de_novo_e_idempotente()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        using var first = NewReplica(database.ConnectionString);
        await first.Coordinator.RunAsync();
        var before = await SnapshotAsync(database.ConnectionString);

        using var second = NewReplica(database.ConnectionString);
        await second.Coordinator.RunAsync();
        var after = await SnapshotAsync(database.ConnectionString);

        after.Should().BeEquivalentTo(before);
    }

    [PostgresFact]
    public async Task Sem_permissao_para_migrar_a_replica_se_recusa_a_subir_ate_o_job_de_migracao_rodar()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();

        using (var replica = NewReplica(database.ConnectionString, ("Startup:RunMigrations", "false")))
        {
            var act = () => replica.Coordinator.RunAsync();
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*--migrate-only*");
        }

        // O job de migração (--migrate-only força RunMigrations=true) prepara o banco...
        using (var job = NewReplica(database.ConnectionString, ("Startup:RunMigrations", "false")))
        {
            job.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<StartupOptions>>().Value.RunMigrations = true;
            await job.Coordinator.RunAsync();
        }

        // ...e a réplica, que segue proibida de migrar, agora sobe.
        using var afterJob = NewReplica(database.ConnectionString, ("Startup:RunMigrations", "false"));
        await afterJob.Coordinator.RunAsync();
    }

    [PostgresFact]
    public async Task Banco_inalcancavel_falha_depois_do_tempo_de_espera_com_erro_claro()
    {
        const string unreachable = "Host=127.0.0.1;Port=1;Database=x;Username=u;Password=p;Timeout=2;";
        using var replica = NewReplica(unreachable, ("Startup:DatabaseWaitTimeoutSeconds", "4"));

        var clock = Stopwatch.StartNew();
        var act = () => replica.Coordinator.RunAsync();

        var thrown = (await act.Should().ThrowAsync<DatabaseUnavailableException>()).Which;
        clock.Elapsed.Should().BeGreaterThan(TimeSpan.FromSeconds(2)).And.BeLessThan(TimeSpan.FromSeconds(20));
        thrown.Message.Should().Contain("Banco indisponível").And.Contain("tentativa(s)");
        thrown.Message.Should().NotContain("Password", "o erro não pode expor a senha");
    }

    [PostgresFact]
    public async Task Banco_inexistente_e_erro_permanente_e_falha_na_hora()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        using var replica = NewReplica(database.ConnectionStringForMissingDatabase(), ("Startup:DatabaseWaitTimeoutSeconds", "60"));

        var clock = Stopwatch.StartNew();
        var act = () => replica.Coordinator.RunAsync();

        await act.Should().ThrowAsync<DatabaseUnavailableException>().WithMessage("*permanente*");
        clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10), "erro permanente não gasta o tempo de espera");
    }

    [PostgresFact]
    public async Task Bloqueio_e_exclusivo_e_passa_para_a_proxima_replica_quando_liberado()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var locker = new PostgresAdvisoryLock(database.ConnectionString, NullLogger<PostgresAdvisoryLock>.Instance);

        var first = await locker.AcquireAsync(TimeSpan.FromSeconds(5), CancellationToken.None);

        var act = () => locker.AcquireAsync(TimeSpan.FromSeconds(2), CancellationToken.None);
        await act.Should().ThrowAsync<TimeoutException>();

        await first.DisposeAsync();
        await using var second = await locker.AcquireAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
    }

    [PostgresFact]
    public async Task Replica_que_morre_segurando_o_bloqueio_nao_o_deixa_preso()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var named = new NpgsqlConnectionStringBuilder(database.ConnectionString) { ApplicationName = "titular-do-bloqueio" }.ConnectionString;
        var holder = new PostgresAdvisoryLock(named, NullLogger<PostgresAdvisoryLock>.Instance);
        var other = new PostgresAdvisoryLock(database.ConnectionString, NullLogger<PostgresAdvisoryLock>.Instance);

        await holder.AcquireAsync(TimeSpan.FromSeconds(5), CancellationToken.None);

        // Simula a morte abrupta da réplica: o servidor derruba a sessão sem que ela tenha chamado unlock.
        await using (var admin = new NpgsqlConnection(database.ConnectionString))
        {
            await admin.OpenAsync();
            await using var kill = new NpgsqlCommand(
                "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE application_name = 'titular-do-bloqueio'", admin);
            await kill.ExecuteNonQueryAsync();
        }

        await using var acquired = await other.AcquireAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
    }

    [PostgresFact]
    public async Task Leitor_de_capacidade_devolve_o_max_connections_real()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();

        var max = await new NpgsqlDatabaseCapacityReader(database.ConnectionString).GetMaxConnectionsAsync(CancellationToken.None);

        max.Should().BeGreaterThan(0);
    }
}

using System.Net.Sockets;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Startup;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace EmpregaNet.Tests.Unit.Persistence.Startup;

public sealed class DatabaseReadinessWaiterTests
{
    private static NpgsqlException Transient() => new("conexão recusada", new SocketException());

    private static (DatabaseReadinessWaiter Waiter, List<TimeSpan> Delays) Build(
        IDatabaseProbe probe, int timeoutSeconds = 30, int maxDelaySeconds = 10)
    {
        var time = new ManualTimeProvider();
        var delays = new List<TimeSpan>();
        var options = Options.Create(new StartupOptions
        {
            DatabaseWaitTimeoutSeconds = timeoutSeconds,
            DatabaseWaitMaxDelaySeconds = maxDelaySeconds
        });

        var waiter = new DatabaseReadinessWaiter(
            probe, options, new ListLogger<DatabaseReadinessWaiter>(), time,
            (span, _) =>
            {
                delays.Add(span);
                time.Advance(span);
                return Task.CompletedTask;
            });

        return (waiter, delays);
    }

    [Fact]
    public async Task Banco_disponivel_de_primeira_nao_espera_nada()
    {
        var probe = new ScriptedProbe();
        var (waiter, delays) = Build(probe);

        await waiter.WaitAsync(CancellationToken.None);

        probe.Calls.Should().Be(1);
        delays.Should().BeEmpty();
    }

    [Fact]
    public async Task Banco_que_demora_a_subir_e_esperado_com_intervalo_crescente_e_com_teto()
    {
        // Falha cinco vezes (o RDS ainda está iniciando) e responde na sexta.
        var probe = new ScriptedProbe(Transient(), Transient(), Transient(), Transient(), Transient(), null);
        var (waiter, delays) = Build(probe, timeoutSeconds: 60, maxDelaySeconds: 10);

        await waiter.WaitAsync(CancellationToken.None);

        probe.Calls.Should().Be(6);
        delays.Select(d => d.TotalSeconds).Should().Equal(1, 2, 4, 8, 10);
    }

    [Fact]
    public async Task Esgotado_o_tempo_lanca_com_tentativas_e_ultimo_erro_sem_mascarar()
    {
        var probe = new ScriptedProbe(Enumerable.Repeat<Exception?>(Transient(), 100).ToArray());
        var (waiter, delays) = Build(probe, timeoutSeconds: 30, maxDelaySeconds: 10);

        var act = () => waiter.WaitAsync(CancellationToken.None);

        var thrown = (await act.Should().ThrowAsync<DatabaseUnavailableException>()).Which;
        thrown.Message.Should().Contain("6 tentativa(s)").And.Contain("limite 30 s").And.Contain("conexão recusada");
        thrown.InnerException.Should().BeOfType<NpgsqlException>();
        delays.Select(d => d.TotalSeconds).Should().Equal(1, 2, 4, 8, 10);
    }

    [Fact]
    public async Task Erro_permanente_falha_na_hora_sem_gastar_o_tempo_de_espera()
    {
        var probe = new ScriptedProbe(new InvalidOperationException("password authentication failed"));
        var (waiter, delays) = Build(probe);

        var act = () => waiter.WaitAsync(CancellationToken.None);

        await act.Should().ThrowAsync<DatabaseUnavailableException>().WithMessage("*permanente*");
        probe.Calls.Should().Be(1);
        delays.Should().BeEmpty();
    }

    [Fact]
    public async Task Cancelamento_interrompe_a_espera()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var waiter = new DatabaseReadinessWaiter(
            new CancelingProbe(), Options.Create(new StartupOptions()), new ListLogger<DatabaseReadinessWaiter>());

        var act = () => waiter.WaitAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private sealed class CancelingProbe : IDatabaseProbe
    {
        public Task ProbeAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void Classifica_falhas_de_rede_como_transitorias_e_o_resto_como_permanente()
    {
        DatabaseReadinessWaiter.IsTransient(Transient()).Should().BeTrue();
        DatabaseReadinessWaiter.IsTransient(new TimeoutException()).Should().BeTrue();
        DatabaseReadinessWaiter.IsTransient(new IOException()).Should().BeTrue();
        DatabaseReadinessWaiter.IsTransient(new InvalidOperationException()).Should().BeFalse();
    }
}

public sealed class StartupCoordinatorTests
{
    private static StartupCoordinator Build(
        List<string> journal, IEnumerable<IStartupStep> steps, Exception? lockFailure = null, StartupOptions? options = null) =>
        new(new RecordingWaiter(journal), new RecordingLock(journal, lockFailure), steps,
            Options.Create(options ?? new StartupOptions()), new ListLogger<StartupCoordinator>());

    [Fact]
    public async Task Espera_o_banco_toma_o_bloqueio_executa_as_etapas_em_ordem_e_so_entao_libera()
    {
        var journal = new List<string>();
        var coordinator = Build(journal, [new RecordingStep("A", journal), new RecordingStep("B", journal)]);

        await coordinator.RunAsync();

        journal.Should().Equal("banco:pronto", "lock:adquirido", "step:A", "step:B", "lock:liberado");
    }

    [Fact]
    public async Task Falha_numa_etapa_libera_o_bloqueio_pula_as_seguintes_e_propaga_o_erro()
    {
        var journal = new List<string>();
        var coordinator = Build(journal,
        [
            new RecordingStep("A", journal),
            new RecordingStep("B", journal, new InvalidOperationException("migração quebrada")),
            new RecordingStep("C", journal)
        ]);

        var act = () => coordinator.RunAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("migração quebrada");
        journal.Should().Equal("banco:pronto", "lock:adquirido", "step:A", "step:B", "lock:liberado");
    }

    [Fact]
    public async Task Sem_conseguir_o_bloqueio_nenhuma_etapa_roda()
    {
        var journal = new List<string>();
        var coordinator = Build(journal, [new RecordingStep("A", journal)], lockFailure: new TimeoutException("outra réplica"));

        var act = () => coordinator.RunAsync();

        await act.Should().ThrowAsync<TimeoutException>();
        journal.Should().Equal("banco:pronto", "lock:falhou");
    }

    [Fact]
    public async Task Opcoes_invalidas_impedem_a_inicializacao_antes_de_tocar_no_banco()
    {
        var journal = new List<string>();
        var coordinator = Build(journal, [], options: new StartupOptions { LockTimeoutSeconds = 0 });

        var act = () => coordinator.RunAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*LockTimeoutSeconds*");
        journal.Should().BeEmpty();
    }
}

public sealed class MigrationStepTests
{
    private static MigrationStep Build(IMigrationRunner runner, bool runMigrations) =>
        new(runner, Options.Create(new StartupOptions { RunMigrations = runMigrations }), new ListLogger<MigrationStep>());

    [Fact]
    public async Task Sem_migracao_pendente_nada_e_aplicado()
    {
        var runner = new FakeMigrationRunner();

        await Build(runner, runMigrations: true).ExecuteAsync(CancellationToken.None);

        runner.Migrated.Should().BeFalse();
    }

    [Fact]
    public async Task Com_migracao_pendente_e_permissao_aplica()
    {
        var runner = new FakeMigrationRunner("20261019_Nova");

        await Build(runner, runMigrations: true).ExecuteAsync(CancellationToken.None);

        runner.Migrated.Should().BeTrue();
    }

    [Fact]
    public async Task Com_migracao_pendente_sem_permissao_a_replica_se_recusa_a_subir_e_nao_migra()
    {
        var runner = new FakeMigrationRunner("20261019_Nova");

        var act = () => Build(runner, runMigrations: false).ExecuteAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*20261019_Nova*RunMigrations=false*--migrate-only*");
        runner.Migrated.Should().BeFalse();
    }

    [Fact]
    public async Task Sem_migracao_pendente_e_sem_permissao_a_replica_sobe_normalmente()
    {
        await Build(new FakeMigrationRunner(), runMigrations: false).ExecuteAsync(CancellationToken.None);
    }
}

public sealed class ConnectionCapacityStepTests
{
    private sealed class FixedReader(int? max, Exception? failure = null) : IDatabaseCapacityReader
    {
        public Task<int?> GetMaxConnectionsAsync(CancellationToken cancellationToken) =>
            failure is null ? Task.FromResult(max) : Task.FromException<int?>(failure);
    }

    private static (ConnectionCapacityStep Step, ListLogger<ConnectionCapacityStep> Log) Build(
        IDatabaseCapacityReader reader, DatabaseOptions? options = null)
    {
        var log = new ListLogger<ConnectionCapacityStep>();
        return (new ConnectionCapacityStep(reader, Options.Create(options ?? new DatabaseOptions()), log), log);
    }

    [Fact]
    public async Task Avisa_quando_o_pool_das_replicas_previstas_nao_cabe_no_banco()
    {
        var (step, log) = Build(new FixedReader(40), new DatabaseOptions { MaxPoolSize = 20, ExpectedMaxReplicas = 2 });

        await step.ExecuteAsync(CancellationToken.None);

        log.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning && e.Message.Contains("max_connections = 40"));
    }

    [Fact]
    public async Task Sem_aviso_quando_cabe()
    {
        var (step, log) = Build(new FixedReader(112));

        await step.ExecuteAsync(CancellationToken.None);

        log.Entries.Should().NotContain(e => e.Level >= LogLevel.Warning);
    }

    [Fact]
    public async Task Falha_ao_ler_o_banco_nao_impede_a_inicializacao()
    {
        var (step, log) = Build(new FixedReader(null, new NpgsqlException("sem permissão")));

        await step.ExecuteAsync(CancellationToken.None);

        log.Entries.Should().Contain(e => e.Level == LogLevel.Warning && e.Message.Contains("capacidade não verificada"));
    }
}

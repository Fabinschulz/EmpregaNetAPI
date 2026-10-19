using Npgsql;

namespace EmpregaNet.Tests.Support;

/// <summary>
/// Teste que precisa de um PostgreSQL de verdade (bloqueio consultivo, migrações, concorrência). Só roda quando
/// <c>EMPREGANET_TEST_PG</c> aponta para um servidor descartável; sem a variável, aparece como ignorado e diz como
/// habilitar. Nunca aponte para um banco compartilhado: cada teste cria e apaga o próprio banco.
/// </summary>
public sealed class PostgresFactAttribute : FactAttribute
{
    public const string EnvironmentVariable = "EMPREGANET_TEST_PG";

    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(EnvironmentVariable)))
        {
            Skip = $"Defina {EnvironmentVariable} com a connection string de administração de um PostgreSQL descartável " +
                   "(ex.: Host=localhost;Port=55432;Username=postgres;Password=test;Database=postgres).";
        }
    }
}

/// <summary>Banco temporário, criado no servidor de teste e apagado ao final.</summary>
internal sealed class PostgresTestDatabase : IAsyncDisposable
{
    private readonly string _adminConnectionString;
    private readonly string _name;

    public string ConnectionString { get; }

    private PostgresTestDatabase(string adminConnectionString, string name, string connectionString)
    {
        _adminConnectionString = adminConnectionString;
        _name = name;
        ConnectionString = connectionString;
    }

    public static async Task<PostgresTestDatabase> CreateAsync()
    {
        var admin = Environment.GetEnvironmentVariable(PostgresFactAttribute.EnvironmentVariable)
                    ?? throw new InvalidOperationException($"{PostgresFactAttribute.EnvironmentVariable} não definida.");
        var name = $"t_{Guid.NewGuid():N}";

        await using (var connection = new NpgsqlConnection(admin))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
            await command.ExecuteNonQueryAsync();
        }

        var builder = new NpgsqlConnectionStringBuilder(admin) { Database = name };
        return new PostgresTestDatabase(admin, name, builder.ConnectionString);
    }

    /// <summary>Connection string do mesmo servidor, para um banco que não existe.</summary>
    public string ConnectionStringForMissingDatabase()
    {
        var builder = new NpgsqlConnectionStringBuilder(_adminConnectionString) { Database = $"nao_existe_{Guid.NewGuid():N}" };
        return builder.ConnectionString;
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();

        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_name}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }
}

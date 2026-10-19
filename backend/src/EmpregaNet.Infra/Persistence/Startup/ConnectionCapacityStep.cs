using EmpregaNet.Infra.Persistence.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace EmpregaNet.Infra.Persistence.Startup;

public interface IDatabaseCapacityReader
{
    Task<int?> GetMaxConnectionsAsync(CancellationToken cancellationToken);
}

public sealed class NpgsqlDatabaseCapacityReader(string connectionString) : IDatabaseCapacityReader
{
    public async Task<int?> GetMaxConnectionsAsync(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("SHOW max_connections", connection);
        return int.TryParse((string?)await command.ExecuteScalarAsync(cancellationToken), out var value) ? value : null;
    }
}

/// <summary>
/// Compara o pool configurado com o <c>max_connections</c> real do banco e avisa se as réplicas previstas não cabem.
/// </summary>
/// <remarks>
/// É um aviso, não uma trava: o valor correto depende do RDS efetivo, que só o banco sabe. Falha de leitura não
/// impede a inicialização.
/// </remarks>
public sealed class ConnectionCapacityStep(
    IDatabaseCapacityReader reader,
    IOptions<DatabaseOptions> options,
    ILogger<ConnectionCapacityStep> logger) : IStartupStep
{
    public string Name => "capacidade de conexões";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var database = options.Value;

        int? maxConnections;
        try
        {
            maxConnections = await reader.GetMaxConnectionsAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning("Não foi possível ler max_connections do banco ({Error}); capacidade não verificada.", ex.Message);
            return;
        }

        if (maxConnections is null)
        {
            return;
        }

        var required = DatabaseCapacity.Required(database.ExpectedMaxReplicas, database.MaxPoolSize, database.ReservedConnections);

        if (required > maxConnections)
        {
            logger.LogWarning(
                "O pool não cabe no banco: {Replicas} réplica(s) x {Pool} + {Reserved} reservadas = {Required} conexões, " +
                "mas max_connections = {Max}. Reduza Database:MaxPoolSize para no máximo {Safe}.",
                database.ExpectedMaxReplicas, database.MaxPoolSize, database.ReservedConnections, required, maxConnections,
                DatabaseCapacity.MaxSafePoolSize(maxConnections.Value, database.ExpectedMaxReplicas, database.ReservedConnections));
            return;
        }

        logger.LogInformation(
            "Capacidade de conexões ok: {Required} necessárias no pior caso, max_connections = {Max}.", required, maxConnections);
    }
}

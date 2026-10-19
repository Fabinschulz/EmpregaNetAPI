using Microsoft.Extensions.Logging;
using Npgsql;

namespace EmpregaNet.Infra.Persistence.Startup;

public interface IStartupLock
{
    Task<IAsyncDisposable> AcquireAsync(TimeSpan timeout, CancellationToken cancellationToken);
}

/// <summary>
/// <c>pg_try_advisory_lock</c> numa conexão dedicada e sem pool: o bloqueio é da sessão, então réplica que morre
/// no meio da inicialização o perde sozinha.
/// </summary>
public sealed class PostgresAdvisoryLock(string connectionString, ILogger<PostgresAdvisoryLock> logger) : IStartupLock
{
    // "EMPR" + "STRT" em ASCII.
    public const long LockKey = 0x454D5052_53545254L;

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    public async Task<IAsyncDisposable> AcquireAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString) { Pooling = false };
        var connection = new NpgsqlConnection(builder.ConnectionString);
        var deadline = DateTime.UtcNow + timeout;

        try
        {
            await connection.OpenAsync(cancellationToken);

            while (true)
            {
                await using (var command = new NpgsqlCommand("SELECT pg_try_advisory_lock(@key)", connection))
                {
                    command.Parameters.AddWithValue("key", LockKey);
                    if ((bool)(await command.ExecuteScalarAsync(cancellationToken))!)
                    {
                        return new Handle(connection);
                    }
                }

                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException(
                        $"Outra réplica manteve o bloqueio de inicialização por mais de {timeout.TotalSeconds:N0} s.");
                }

                logger.LogInformation("Outra réplica está inicializando o banco; aguardando.");
                await Task.Delay(PollInterval, cancellationToken);
            }
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private sealed class Handle(NpgsqlConnection connection) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            try
            {
                await using var command = new NpgsqlCommand("SELECT pg_advisory_unlock(@key)", connection);
                command.Parameters.AddWithValue("key", LockKey);
                await command.ExecuteScalarAsync();
            }
            catch
            {
                // Fechar a conexão libera o bloqueio de qualquer forma.
            }

            await connection.DisposeAsync();
        }
    }
}

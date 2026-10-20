using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace EmpregaNet.Infra.Persistence.Startup;

public interface IDatabaseProbe
{
    Task ProbeAsync(CancellationToken cancellationToken);
}

public sealed class NpgsqlDatabaseProbe(string connectionString) : IDatabaseProbe
{
    public async Task ProbeAsync(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync(cancellationToken);
    }
}

public sealed class DatabaseUnavailableException(string message, Exception inner) : Exception(message, inner);

public interface IDatabaseReadinessWaiter
{
    Task WaitAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Espera o PostgreSQL com intervalo crescente e limite de tempo (o RDS religa pouco antes das tarefas).
/// Erro permanente (senha errada, banco inexistente) falha na hora; esgotado o tempo, lança sem mascarar o erro.
/// </summary>
public sealed class DatabaseReadinessWaiter : IDatabaseReadinessWaiter
{
    private readonly IDatabaseProbe _probe;
    private readonly StartupOptions _options;
    private readonly ILogger<DatabaseReadinessWaiter> _logger;
    private readonly TimeProvider _time;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;

    public DatabaseReadinessWaiter(
        IDatabaseProbe probe,
        IOptions<StartupOptions> options,
        ILogger<DatabaseReadinessWaiter> logger,
        TimeProvider? timeProvider = null,
        Func<TimeSpan, CancellationToken, Task>? delay = null)
    {
        _probe = probe;
        _options = options.Value;
        _logger = logger;
        _time = timeProvider ?? TimeProvider.System;
        _delay = delay ?? ((span, token) => Task.Delay(span, token));
    }

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        var timeout = TimeSpan.FromSeconds(_options.DatabaseWaitTimeoutSeconds);
        var maxDelay = TimeSpan.FromSeconds(_options.DatabaseWaitMaxDelaySeconds);
        var started = _time.GetTimestamp();
        var delay = TimeSpan.FromSeconds(1);
        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                await _probe.ProbeAsync(cancellationToken);

                if (attempt > 1)
                {
                    _logger.LogInformation(
                        "Banco disponível na tentativa {Attempt}, após {Elapsed:N0} s.",
                        attempt, _time.GetElapsedTime(started).TotalSeconds);
                }

                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (!IsTransient(ex))
                {
                    throw new DatabaseUnavailableException(
                        $"Falha permanente ao conectar no banco ({ex.GetType().Name}: {ex.Message}). Não adianta esperar.", ex);
                }

                var elapsed = _time.GetElapsedTime(started);
                if (elapsed + delay > timeout)
                {
                    throw new DatabaseUnavailableException(
                        $"Banco indisponível depois de {attempt} tentativa(s) em {elapsed.TotalSeconds:N0} s " +
                        $"(limite {timeout.TotalSeconds:N0} s). Último erro: {ex.GetType().Name}: {ex.Message}", ex);
                }

                _logger.LogWarning(
                    "Banco indisponível (tentativa {Attempt}); nova tentativa em {Delay:N0} s. {ErrorType}: {Error}",
                    attempt, delay.TotalSeconds, ex.GetType().Name, ex.Message);

                await _delay(delay, cancellationToken);
                delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, maxDelay.Ticks));
            }
        }
    }

    public static bool IsTransient(Exception exception) => exception switch
    {
        NpgsqlException npgsql => npgsql.IsTransient,
        TimeoutException or SocketException or IOException => true,
        _ => false
    };
}

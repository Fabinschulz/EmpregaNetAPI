using EmpregaNet.Infra.Persistence.Startup;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Tests.Support;

internal sealed class ManualTimeProvider : TimeProvider
{
    private long _ticks;

    public override long GetTimestamp() => _ticks;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public void Advance(TimeSpan span) => _ticks += span.Ticks;
}

internal sealed class ListLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Entries.Add((logLevel, formatter(state, exception)));
}

internal sealed class ScriptedProbe(params Exception?[] results) : IDatabaseProbe
{
    private readonly Queue<Exception?> _results = new(results);

    public int Calls { get; private set; }

    public Task ProbeAsync(CancellationToken cancellationToken)
    {
        Calls++;
        var next = _results.Count > 0 ? _results.Dequeue() : null;
        return next is null ? Task.CompletedTask : Task.FromException(next);
    }
}

internal sealed class RecordingStep(string name, List<string> journal, Exception? failure = null) : IStartupStep
{
    public string Name => name;

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        journal.Add($"step:{name}");
        return failure is null ? Task.CompletedTask : Task.FromException(failure);
    }
}

internal sealed class RecordingLock(List<string> journal, Exception? acquireFailure = null) : IStartupLock
{
    public Task<IAsyncDisposable> AcquireAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (acquireFailure is not null)
        {
            journal.Add("lock:falhou");
            return Task.FromException<IAsyncDisposable>(acquireFailure);
        }

        journal.Add("lock:adquirido");
        return Task.FromResult<IAsyncDisposable>(new Releaser(journal));
    }

    private sealed class Releaser(List<string> journal) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            journal.Add("lock:liberado");
            return ValueTask.CompletedTask;
        }
    }
}

internal sealed class RecordingWaiter(List<string> journal) : IDatabaseReadinessWaiter
{
    public Task WaitAsync(CancellationToken cancellationToken)
    {
        journal.Add("banco:pronto");
        return Task.CompletedTask;
    }
}

internal sealed class FakeMigrationRunner(params string[] pending) : IMigrationRunner
{
    public bool Migrated { get; private set; }

    public Task<IReadOnlyList<string>> GetPendingAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<string>>(Migrated ? [] : pending);

    public Task MigrateAsync(CancellationToken cancellationToken)
    {
        Migrated = true;
        return Task.CompletedTask;
    }
}

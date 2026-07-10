using System.Collections.Concurrent;
using EmpregaNet.Application.Abstraction;
using StackExchange.Redis;

namespace EmpregaNet.Infra.Email;

/// <summary>
/// Teto diário de e-mails por destinatário via Redis (INCR atômico com expiração).
/// Sobrevive a restarts e vale para múltiplas instâncias da API.
/// </summary>
public sealed class RedisEmailThrottleService : IEmailThrottleService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly int _maxPerDay;

    public RedisEmailThrottleService(IConnectionMultiplexer redis, int maxPerDay)
    {
        _redis = redis;
        _maxPerDay = maxPerDay;
    }

    public async Task<bool> TryAcquireAsync(string email, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = BuildKey(email);

        var count = await db.StringIncrementAsync(key);
        if (count == 1)
        {
            // Primeira ocorrência do dia: expira em 25h (folga sobre a virada do dia UTC).
            await db.KeyExpireAsync(key, TimeSpan.FromHours(25));
        }

        return count <= _maxPerDay;
    }

    private static string BuildKey(string email) =>
        $"EmpregaNet:email-throttle:{email.Trim().ToLowerInvariant()}:{DateTime.UtcNow:yyyyMMdd}";
}

/// <summary>
/// Fallback em memória quando o Redis está desabilitado (instância única).
/// </summary>
public sealed class InMemoryEmailThrottleService : IEmailThrottleService
{
    private readonly ConcurrentDictionary<string, int> _counts = new();
    private readonly int _maxPerDay;
    private string _currentDay = DateTime.UtcNow.ToString("yyyyMMdd");

    public InMemoryEmailThrottleService(int maxPerDay)
    {
        _maxPerDay = maxPerDay;
    }

    public Task<bool> TryAcquireAsync(string email, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        if (today != _currentDay)
        {
            // Virou o dia UTC: zera os contadores (evita crescimento indefinido do dicionário).
            _counts.Clear();
            _currentDay = today;
        }

        var count = _counts.AddOrUpdate(email.Trim().ToLowerInvariant(), 1, (_, current) => current + 1);
        return Task.FromResult(count <= _maxPerDay);
    }
}

using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace EmpregaNet.Api.Controllers.HealthChecks;

public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await _redis.GetDatabase().PingAsync();

            if (latency == TimeSpan.Zero)
            {
                _logger.LogWarning("Redis respondeu com latência zero.");
                return HealthCheckResult.Unhealthy("Redis indisponível.");
            }

            return HealthCheckResult.Healthy($"Redis disponível (latência {latency.TotalMilliseconds:F0} ms).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha no health check do Redis.");
            return HealthCheckResult.Unhealthy("Redis indisponível.", ex);
        }
    }
}

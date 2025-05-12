using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

public class MemoryServiceCheck : IHealthCheck
{
    readonly IConnectionMultiplexer _redis;

    readonly ILogger<MemoryServiceCheck> _logger;

    public MemoryServiceCheck(IConnectionMultiplexer connectionMultiplexer, ILogger<MemoryServiceCheck> logger)
    {
        _logger = logger;
        _redis = connectionMultiplexer;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("starting healthcheck");
        try
        {
            var connected = await _redis.GetDatabase().PingAsync();

            if (connected != TimeSpan.Zero)
            {
                _logger.LogDebug("healthcheck success");
                return HealthCheckResult.Healthy($"MemoryService is running: Latency {connected.TotalMilliseconds}ms");
            }
            else
            {
                _logger.LogCritical("MemoryService not running");
                return HealthCheckResult.Unhealthy("MemoryService is not running");
            }

        }
        catch (Exception ex)
        {
            _logger.LogCritical($"MemoryService not running: {ex.Message}");
            return HealthCheckResult.Unhealthy("MemoryService is not running");
        }

    }
}
using Microsoft.Extensions.Diagnostics.HealthChecks;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;

public class DatabaseCheck : IHealthCheck
{
    
    private readonly PostgreSqlContext _context;
    private readonly ILogger<DatabaseCheck> _logger;

    public DatabaseCheck(PostgreSqlContext context, ILogger<DatabaseCheck> logger)        
    {
        _context = context;
        _logger = logger;
    }    
    
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {  
        _logger.LogDebug("starting healthcheck");
        try
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State == System.Data.ConnectionState.Open)
            {
                _logger.LogDebug("healthcheck success");
                return Task.FromResult(HealthCheckResult.Healthy("Database is running"));
            }
            else
            {
                _logger.LogCritical("Database not running");
                return Task.FromResult(HealthCheckResult.Unhealthy("Database is not running"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Database not running: {ex.Message}");
            return Task.FromResult(HealthCheckResult.Unhealthy("Database is not running"));
        }
    }
}
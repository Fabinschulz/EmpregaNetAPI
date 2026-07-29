using Microsoft.Extensions.Diagnostics.HealthChecks;
using EmpregaNet.Infra.Persistence.Database;


public class DatabaseCheck : IHealthCheck
{
    private readonly PostgreSqlContext _context;
    private readonly ILogger<DatabaseCheck> _logger;

    public DatabaseCheck(PostgreSqlContext context, ILogger<DatabaseCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            if (canConnect)
            {
                return HealthCheckResult.Healthy("Banco de dados acessível.");
            }

            _logger.LogCritical("Health check: banco de dados inacessível.");
            return HealthCheckResult.Unhealthy("Banco de dados inacessível.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Health check: falha ao conectar no banco de dados.");
            return HealthCheckResult.Unhealthy("Banco de dados inacessível.", ex);
        }
    }
}

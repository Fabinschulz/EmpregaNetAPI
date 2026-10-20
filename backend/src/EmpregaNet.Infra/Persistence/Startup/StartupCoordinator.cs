using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Infra.Persistence.Startup;

/// <summary>
/// Espera o banco, toma o bloqueio entre réplicas e executa as etapas em ordem. O host só escuta depois disto;
/// qualquer falha libera o bloqueio e propaga, e a aplicação não sobe.
/// </summary>
public sealed class StartupCoordinator(
    IDatabaseReadinessWaiter waiter,
    IStartupLock startupLock,
    IEnumerable<IStartupStep> steps,
    IOptions<StartupOptions> options,
    ILogger<StartupCoordinator> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        settings.Validate();

        await waiter.WaitAsync(cancellationToken);

        await using (await startupLock.AcquireAsync(TimeSpan.FromSeconds(settings.LockTimeoutSeconds), cancellationToken))
        {
            foreach (var step in steps)
            {
                logger.LogInformation("Inicialização: {Step}", step.Name);

                try
                {
                    await step.ExecuteAsync(cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Inicialização falhou na etapa '{Step}'.", step.Name);
                    throw;
                }
            }
        }

        logger.LogInformation("Inicialização concluída.");
    }
}

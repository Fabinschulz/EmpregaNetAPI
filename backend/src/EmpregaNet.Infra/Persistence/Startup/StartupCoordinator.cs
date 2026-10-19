using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Infra.Persistence.Startup;

/// <summary>
/// Orquestra a inicialização que não pode rodar em paralelo entre réplicas: espera o banco, toma o bloqueio e
/// executa as etapas em ordem (migrações, seed, chaves do Data Protection, capacidade).
/// </summary>
/// <remarks>
/// O host só começa a atender depois que isto termina, então nenhuma réplica recebe tráfego antes de estar apta.
/// Qualquer falha propaga: o bloqueio é liberado e a aplicação não sobe, para o orquestrador reiniciar ou reverter.
/// </remarks>
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

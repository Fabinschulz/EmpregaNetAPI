using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Infra.Persistence.Startup;

/// <summary>
/// Cria a primeira chave do key ring sob o bloqueio: sem isso, duas réplicas subindo juntas num banco vazio criariam
/// uma chave cada e, até o cache atualizar, protegeriam tokens com chaves que a outra não conhece.
/// </summary>
public sealed class DataProtectionBootstrapStep(
    IDataProtectionProvider provider,
    ILogger<DataProtectionBootstrapStep> logger) : IStartupStep
{
    public string Name => "chaves do Data Protection";

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Proteger qualquer valor força o framework a ler o key ring e criar a chave, se não houver.
        provider.CreateProtector("EmpregaNet.Startup").Protect("bootstrap");
        logger.LogInformation("Key ring do Data Protection verificado.");
        return Task.CompletedTask;
    }
}

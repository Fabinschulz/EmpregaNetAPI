using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Infra.Persistence.Startup;

/// <summary>
/// Garante que o key ring existe antes de qualquer réplica atender tráfego.
/// </summary>
/// <remarks>
/// Sem isto, duas réplicas que sobem juntas num banco vazio criariam uma chave cada. O framework tolera (ambas
/// ficam no key ring), mas até a atualização do cache cada réplica protegeria tokens com uma chave que a outra
/// ainda não conhece. Sob o bloqueio de inicialização, só a primeira cria; as demais encontram a chave pronta.
/// Se já existe chave ativa, nada é criado, apagado ou trocado.
/// </remarks>
public sealed class DataProtectionBootstrapStep(
    IDataProtectionProvider provider,
    ILogger<DataProtectionBootstrapStep> logger) : IStartupStep
{
    public string Name => "chaves do Data Protection";

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Proteger qualquer valor obriga o framework a ler o key ring e criar a primeira chave, se ainda não houver.
        provider.CreateProtector("EmpregaNet.Startup").Protect("bootstrap");
        logger.LogInformation("Key ring do Data Protection verificado.");
        return Task.CompletedTask;
    }
}

using EmpregaNet.Infra.Persistence.Seeds;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Infra.Persistence.Startup;

/// <summary>
/// Roles e administrador iniciais. O seeder verifica antes de criar; duas réplicas fazendo isso ao mesmo tempo
/// criariam a mesma role duas vezes e duplicariam as permissões do Admin (a tabela de claims não tem índice único).
/// Por isso só roda aqui, sob o bloqueio de inicialização.
/// </summary>
public sealed class SeedStep(IServiceProvider services, IOptions<StartupOptions> options) : IStartupStep
{
    public string Name => "seed";

    public Task ExecuteAsync(CancellationToken cancellationToken) =>
        options.Value.RunSeed ? IdentityDataSeeder.SeedAsync(services, cancellationToken) : Task.CompletedTask;
}

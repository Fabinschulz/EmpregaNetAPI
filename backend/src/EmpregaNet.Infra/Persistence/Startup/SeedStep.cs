using EmpregaNet.Infra.Persistence.Seeds;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Infra.Persistence.Startup;

/// <summary>
/// Só roda sob o bloqueio: o seeder verifica antes de criar, e duas réplicas em paralelo duplicariam as permissões
/// do Admin (a tabela de claims não tem índice único).
/// </summary>
public sealed class SeedStep(IServiceProvider services, IOptions<StartupOptions> options) : IStartupStep
{
    public string Name => "seed";

    public Task ExecuteAsync(CancellationToken cancellationToken) =>
        options.Value.RunSeed ? IdentityDataSeeder.SeedAsync(services, cancellationToken) : Task.CompletedTask;
}

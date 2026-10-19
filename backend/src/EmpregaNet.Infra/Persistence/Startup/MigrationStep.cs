using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Infra.Persistence.Startup;

/// <summary>Uma etapa da inicialização, executada sob o bloqueio entre réplicas.</summary>
public interface IStartupStep
{
    string Name { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}

public interface IMigrationRunner
{
    Task<IReadOnlyList<string>> GetPendingAsync(CancellationToken cancellationToken);

    Task MigrateAsync(CancellationToken cancellationToken);
}

/// <summary>Executa as migrações do EF Core. Provedores não relacionais (testes em memória) não têm migrações.</summary>
public sealed class EfMigrationRunner(IServiceScopeFactory scopeFactory) : IMigrationRunner
{
    public async Task<IReadOnlyList<string>> GetPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        if (!context.Database.IsRelational())
        {
            return [];
        }

        return (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
    }

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}

/// <summary>
/// Aplica as migrações pendentes, ou recusa a inicialização quando a política é não aplicá-las.
/// </summary>
/// <remarks>
/// Com <see cref="StartupOptions.RunMigrations"/> = false, uma réplica que encontra o schema atrasado não atende
/// tráfego: servir código novo sobre schema velho é pior que não subir. O erro não é mascarado.
/// </remarks>
public sealed class MigrationStep(
    IMigrationRunner runner,
    IOptions<StartupOptions> options,
    ILogger<MigrationStep> logger) : IStartupStep
{
    public string Name => "migrações";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var pending = await runner.GetPendingAsync(cancellationToken);

        if (pending.Count == 0)
        {
            logger.LogInformation("Nenhuma migração pendente.");
            return;
        }

        if (!options.Value.RunMigrations)
        {
            throw new InvalidOperationException(
                $"Há {pending.Count} migração(ões) pendente(s) ({string.Join(", ", pending)}) e " +
                $"{StartupOptions.SectionName}:{nameof(StartupOptions.RunMigrations)}=false. " +
                "Execute o job de migração (dotnet EmpregaNet.Api.dll --migrate-only) antes de iniciar as réplicas.");
        }

        logger.LogInformation("Aplicando {Count} migração(ões): {Migrations}", pending.Count, string.Join(", ", pending));
        await runner.MigrateAsync(cancellationToken);
        logger.LogInformation("Migrações aplicadas.");
    }
}

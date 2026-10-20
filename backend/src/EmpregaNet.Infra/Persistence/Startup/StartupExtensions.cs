using EmpregaNet.Infra.Persistence.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Infra.Persistence.Startup;

public static class StartupExtensions
{
    public const string MigrateOnlyArgument = "--migrate-only";

    public static IServiceCollection AddStartupTasks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = PostgreSqlConnectionString.Resolve(configuration);

        services.AddOptions<StartupOptions>().Bind(configuration.GetSection(StartupOptions.SectionName));
        services.AddOptions<DatabaseOptions>().Bind(configuration.GetSection(DatabaseOptions.SectionName));
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IDatabaseProbe>(new NpgsqlDatabaseProbe(connectionString));
        services.AddSingleton<IDatabaseCapacityReader>(new NpgsqlDatabaseCapacityReader(connectionString));
        services.AddSingleton<IStartupLock>(sp =>
            new PostgresAdvisoryLock(connectionString, sp.GetRequiredService<ILogger<PostgresAdvisoryLock>>()));
        services.AddSingleton<IDatabaseReadinessWaiter, DatabaseReadinessWaiter>();
        services.AddSingleton<IMigrationRunner, EfMigrationRunner>();

        // A ordem importa: o schema precisa existir antes do seed e do key ring, que gravam nele.
        services.AddSingleton<IStartupStep, MigrationStep>();
        services.AddSingleton<IStartupStep, SeedStep>();
        services.AddSingleton<IStartupStep, DataProtectionBootstrapStep>();
        services.AddSingleton<IStartupStep, ConnectionCapacityStep>();

        services.AddSingleton<StartupCoordinator>();
        return services;
    }

    /// <summary>Com <paramref name="migrateOnly"/> as migrações são aplicadas mesmo com <c>Startup:RunMigrations</c> = false.</summary>
    public static async Task RunStartupTasksAsync(
        this WebApplication app,
        bool migrateOnly = false,
        CancellationToken cancellationToken = default)
    {
        if (migrateOnly)
        {
            app.Services.GetRequiredService<IOptions<StartupOptions>>().Value.RunMigrations = true;
            app.Logger.LogInformation("Modo {Argument}: aplica a inicialização e encerra.", MigrateOnlyArgument);
        }

        await app.Services.GetRequiredService<StartupCoordinator>().RunAsync(cancellationToken);
    }
}

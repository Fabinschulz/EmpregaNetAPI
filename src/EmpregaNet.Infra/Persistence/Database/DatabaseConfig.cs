using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Persistence.Database;

public static class DatabaseConfig
{
    public static WebApplicationBuilder SetupDatabaseConnection(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("PostgreSQLConnection")
                                            ?? throw new Exception("PostgreSQLConnection não encontrada no arquivo de configuração.");

        Console.WriteLine("Initializing Database for API: " + connectionString.Substring(0, 49));
        try
        {
            builder.Services.AddDbContext<PostgreSqlContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(PostgreSqlContext).Assembly.GetName().Name);
                    npgsqlOptions.EnableRetryOnFailure(
                               maxRetryCount: 2,
                               maxRetryDelay: TimeSpan.FromSeconds(10),
                               errorCodesToAdd: null);
                })
                        .EnableDetailedErrors()
                        .EnableSensitiveDataLogging(true);
            });

            Console.WriteLine("Database connection established successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error connecting to database: " + e.Message);
            throw new Exception("Error on postgresql: " + connectionString.Substring(0, 49));
        }

        return builder;
    }

    /// <summary>
    /// Aplica migrações pendentes no banco de dados.
    /// </summary>
    /// <param name="webApp"></param>
    public static WebApplication ApplyPendingMigrations(this WebApplication webApp)
    {
        using var scope = webApp.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }

        return webApp;
    }
}
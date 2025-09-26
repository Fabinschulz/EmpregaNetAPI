using EmpregaNet.Infra.Persistence.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Configurations;

public static class DatabaseConfig
{
    public static void SetUpDatabaseConnection(this WebApplicationBuilder builder)
    {
        string connectionString = builder.Configuration.GetConnectionString("PostgreSQLConnection")!;
        Console.WriteLine("Initializing Database for API: " + connectionString.Substring(0, 49));

        try
        {
            builder.Services.AddDbContext<PostgreSqlContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
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
    }

    public static void ApplyMigrations(this IServiceCollection services)
    {
        var postgreSql = GetPostgreSql(services);
        try
        {
            postgreSql.MigrateAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Não foi possível concluir a migração do DB." + ex.ToString());
            throw;
        }
    }

    private static PostgreSqlContext GetPostgreSql(IServiceCollection services)
    {
        return (PostgreSqlContext)(services.BuildServiceProvider().GetService(typeof(PostgreSqlContext))
            ?? throw new InvalidOperationException("PostgreSqlContext service is not registered."));
    }
}

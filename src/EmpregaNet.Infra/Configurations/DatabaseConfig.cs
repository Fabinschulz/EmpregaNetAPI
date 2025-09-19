using EmpregaNet.Infra.Persistence.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Configurations;

public static class DatabaseConfig
{
    public static void AddDatabase(this WebApplicationBuilder builder)
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
}

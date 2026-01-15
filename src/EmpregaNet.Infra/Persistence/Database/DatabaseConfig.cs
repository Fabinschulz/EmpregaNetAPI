using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace EmpregaNet.Infra.Persistence.Database;

public static class DatabaseConfig
{
    public static void SetupDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQLConnection") ?? throw new Exception("PostgreSQLConnection não encontrada no arquivo de configuração.");
        Console.WriteLine("Initializing Database for API: " + connectionString.Substring(0, 49));
        try
        {
            services.AddDbContext<PostgreSqlContext>(options =>
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
    }


    public static void ApplyPendingMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
}
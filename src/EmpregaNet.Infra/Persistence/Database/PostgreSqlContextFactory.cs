using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EmpregaNet.Infra.Persistence.Database
{
    public class PostgreSqlContextFactory : IDesignTimeDbContextFactory<PostgreSqlContext>
    {
        public PostgreSqlContext CreateDbContext(string[] args)
        {

            var basePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "EmpregaNet.Api"
            );

            Console.WriteLine($"Tentando carregar appsettings.json de: {basePath}");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true, true)
                .Build();

            var connectionString = configuration.GetConnectionString("PostgreSQLConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgreSQLConnection' not found in appsettings.json.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlContext>();
            Console.WriteLine($"Usando connection string: {connectionString}");
            optionsBuilder.UseNpgsql(connectionString);

            return new PostgreSqlContext(optionsBuilder.Options);
        }
    }
}
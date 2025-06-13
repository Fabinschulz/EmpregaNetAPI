using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EmpregaNet.Infra.Persistence.Database
{
    public class PostgreSqlContextFactory : IDesignTimeDbContextFactory<PostgreSqlContext>
    {
        public PostgreSqlContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "EmpregaNet.Api");
            if (!Directory.Exists(basePath) || !File.Exists(Path.Combine(basePath, "appsettings.json")))
            {
                basePath = Directory.GetCurrentDirectory();
            }

            Console.WriteLine($"Carregando configurações a partir de: {basePath}");
            Console.WriteLine($"Ambiente ASPNETCORE_ENVIRONMENT: {environmentName ?? "Não definido (usando Production)"}");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName ?? "Production"}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("PostgreSQLConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgreSQLConnection' not found in appsettings.json.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlContext>();
            Console.WriteLine($"Usando connection string para migrações/atualizações: {connectionString}");
            optionsBuilder.UseNpgsql(connectionString);

            return new PostgreSqlContext(optionsBuilder.Options);
        }
    }
}
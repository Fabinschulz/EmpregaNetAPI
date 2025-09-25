using EmpregaNet.Infra.Persistence.Database;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Configurations;

public static class ApplyMigrationsConfig
{
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
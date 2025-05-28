using EmpregaNet.Infra.Persistence.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Configurations;

public static class ApplyMigrationsConfig
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using PostgreSqlContext dbContext = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();
        dbContext.Database.Migrate();
    }

}

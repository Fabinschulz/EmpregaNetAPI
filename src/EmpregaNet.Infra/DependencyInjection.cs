using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Cache.MemoryService;
using EmpregaNet.Infra.Cache.RedisServiceCollection;
using EmpregaNet.Infra.Configurations;
using EmpregaNet.Infra.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra;

public static class DependencyInjection
{

    public static void RegisterCoreDependencies(this WebApplicationBuilder builder)
    {
        builder.AddDatabase();
        builder.AddIdentityConfiguration();
        builder.Services.SetupInfrastructureServices(builder.Configuration);
        builder.AddSentryConfiguration();
        builder.Build().UseSentryTracingMiddleware();
    }

    private static void RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        // services.AddTransient<IEmailSender<User>, IdentityNoOpEmailSender>();
        // services.AddTransient<IEmailSender<User>, EmailSender>();
        services.AddSingleton<IMemoryService, MemoryService>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
    }

    private static void SetupInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.UseRedisCache(configuration);
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddEndpointsApiExplorer();
        services.RegisterRepositories();
        services.AddProblemDetails();
        services.ApplyMigrations();
    }

}

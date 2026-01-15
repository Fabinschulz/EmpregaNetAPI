using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Behaviors;
using EmpregaNet.Infra.Cache;
using EmpregaNet.Infra.Configurations;
using EmpregaNet.Infra.Extensions;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra;

public static class DependencyInjection
{

    public static void RegisterCoreDependencies(this WebApplicationBuilder builder)
    {
        builder.AddIdentityConfiguration();
        builder.Services.SetupInfrastructureServices(builder.Configuration);
        builder.SetupSentryLogging();
        // builder.AddElasticsearch();
    }

    private static void SetupInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.UseRedisCache(configuration);
        services.UseMemoryService(opt => opt.KeyPrefix = "EmpregaNet_Cache_");
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddProblemDetails();
        services.SetupDependencyInjection();
        services.SetupDatabase(configuration);
        services.SetupRateLimiter(configuration);
        // services.SetupAWSCloudWatchLogging(configuration);
    }

    private static void SetupDependencyInjection(this IServiceCollection services)
    {
        // services.AddTransient<IEmailSender<User>, IdentityNoOpEmailSender>();
        // services.AddTransient<IEmailSender<User>, EmailSender>();

        // Adiciona o comportamento de validação antes da execução de qualquer Handler.
        // Ele intercepta a requisição, executa as validações necessárias e, se falhar, evita que o Handler seja executado.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        // services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        services.AddScoped<IUnityOfWork, UnityOfWork>();

        #region Repositories
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        #endregion
    }

}

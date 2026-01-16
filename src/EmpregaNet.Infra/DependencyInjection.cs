using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Behaviors;
using EmpregaNet.Infra.Cache;
using EmpregaNet.Infra.Configurations;
using EmpregaNet.Infra.Extensions;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra;

public static class DependencyInjection
{

    public static void RegisterCoreDependencies(this WebApplicationBuilder builder)
    {
        builder.AddIdentityConfiguration();
        builder.SetupSentryLogging();
        builder.SetupDatabaseConnection();
        builder.SetupInfrastructureServices();
        // builder.AddElasticsearch();
    }

    private static void SetupInfrastructureServices(this WebApplicationBuilder builder)
    {
        builder.UseRedisCache();
        builder.UseMemoryService(opt => opt.KeyPrefix = "EmpregaNet_Cache_");
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddProblemDetails();
        builder.Services.SetupDependencyInjection();
        builder.Services.SetupRateLimiter(builder.Configuration);
        // builder.Services.SetupAWSCloudWatchLogging(builder.Configuration);
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

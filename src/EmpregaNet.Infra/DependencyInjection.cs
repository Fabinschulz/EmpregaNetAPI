using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Behaviors;
using EmpregaNet.Infra.Cache;
using EmpregaNet.Infra.Extensions;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using EmpregaNet.Infra.Email;
using EmpregaNet.Infra.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmpregaNet.Infra;

public static class DependencyInjection
{

    public static void RegisterCoreDependencies(this WebApplicationBuilder builder)
    {
        builder.AddIdentityConfiguration();
        builder.SetupSentryLogging();
        builder.SetupDatabaseConnection();
        builder.SetupInfrastructureServices();
        builder.RegisterAuthFlowServices();
        // builder.AddElasticsearch();
    }

    private static void RegisterAuthFlowServices(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AppUrlsOptions>(builder.Configuration.GetSection(AppUrlsOptions.SectionName));
        builder.Services.Configure<GoogleAuthOptions>(builder.Configuration.GetSection(GoogleAuthOptions.SectionName));
        builder.Services.Configure<SmtpEmailOptions>(builder.Configuration.GetSection(SmtpEmailOptions.SectionName));

        var smtp = builder.Configuration.GetSection(SmtpEmailOptions.SectionName).Get<SmtpEmailOptions>() ?? new SmtpEmailOptions();
        EnsureSmtpConfiguredForProduction(builder.Environment, smtp);

        if (smtp.Enabled && !string.IsNullOrWhiteSpace(smtp.Host) && !string.IsNullOrWhiteSpace(smtp.FromEmail))
            builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
        else
            //OBS: Em desenvolvimento, ou se Smtp:Enabled=false, usa um no-op para evitar erros de configuração.
            builder.Services.AddTransient<IEmailSender, NoOpEmailSender>();

        builder.Services.AddScoped<IGoogleIdTokenValidator, GoogleIdTokenValidator>();
        builder.Services.AddScoped<IAccountEmailService, AccountEmailService>();
    }

    private static void EnsureSmtpConfiguredForProduction(IHostEnvironment env, SmtpEmailOptions smtp)
    {
        if (!env.IsProduction())
            return;

        if (!smtp.Enabled)
        {
            throw new InvalidOperationException(
                "Atenção! Em Produção: Smtp:Enabled deve ser true para envio real de e-mails. Configure as variáveis de ambiente ou ajuste appsettings.");}

        if (string.IsNullOrWhiteSpace(smtp.Host) || string.IsNullOrWhiteSpace(smtp.FromEmail))
        {
            throw new InvalidOperationException(
                "Produção: Smtp:Host e Smtp:FromEmail são obrigatórios quando Smtp:Enabled for true.");
        }
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
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        #endregion
    }

}

using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Behaviors;
using EmpregaNet.Infra.Cache;
using EmpregaNet.Infra.Extensions;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using EmpregaNet.Infra.Persistence.Repositories.Dashboard;
using EmpregaNet.Infra.Email;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace EmpregaNet.Infra;

public static class DependencyInjection
{

    public static void RegisterCoreDependencies(this WebApplicationBuilder builder)
    {
        EnsureJwtKeyIsStrongEnough(builder.Configuration);
        builder.AddIdentityConfiguration();
        builder.SetupSentryLogging();
        builder.SetupDatabaseConnection();
        builder.SetupInfrastructureServices();
        builder.RegisterAuthFlowServices();
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
        builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // Teto diário de e-mails por destinatário (anti-abuso de forgot-password/resend-confirmation).
        // Usa Redis quando ativo (multi-instância/persistente); senão, contador em memória.
        var maxEmailsPerDay = builder.Configuration.GetValue("EmailThrottle:MaxPerDay", 5);
        builder.Services.AddSingleton<IEmailThrottleService>(sp =>
            sp.GetService<IConnectionMultiplexer>() is { } redis
                ? new RedisEmailThrottleService(redis, maxEmailsPerDay)
                : (IEmailThrottleService)new InMemoryEmailThrottleService(maxEmailsPerDay));
    }

    /// <summary>
    /// Recusa subir com uma chave de assinatura curta demais.
    /// </summary>
    /// <remarks>
    /// A validação existente cobria apenas a presença da <c>SecretKey</c>. Uma chave curta passa
    /// nessa checagem e o HMAC-SHA256 assina com ela normalmente, o token continua válido, só que
    /// viável de quebrar por força bruta. Como essa chave é o único segredo entre um anônimo e um
    /// administrador, o comprimento precisa falhar no boot, não numa auditoria.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Chave ausente ou com menos de 32 bytes.</exception>
    private static void EnsureJwtKeyIsStrongEnough(IConfiguration configuration)
    {
        const int minimumSecretKeyBytes = 32;

        var secretKey = configuration["JwtSettings:SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException(
                "'JwtSettings:SecretKey' não configurada. Defina via variável de ambiente (JwtSettings__SecretKey) ou user-secrets.");
        }

        var keyBytes = System.Text.Encoding.UTF8.GetByteCount(secretKey);

        if (keyBytes < minimumSecretKeyBytes)
        {
            throw new InvalidOperationException(
                $"'JwtSettings:SecretKey' tem {keyBytes} bytes; HMAC-SHA256 exige no mínimo {minimumSecretKeyBytes} " +
                "para que a chave não seja o elo fraco da assinatura.");
        }
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
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddProblemDetails();
        builder.Services.SetupDependencyInjection();
        builder.Services.SetupRateLimiter(builder.Configuration);
        // builder.Services.SetupAWSCloudWatchLogging(builder.Configuration);
    }

    private static void SetupDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped<IUnityOfWork, UnityOfWork>();

        #region Repositories
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IDashboardAnalyticsRepository, DashboardAnalyticsRepository>();
        #endregion
    }

}

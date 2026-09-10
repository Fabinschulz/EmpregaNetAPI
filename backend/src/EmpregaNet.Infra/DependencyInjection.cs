using Amazon;
using Amazon.SimpleEmailV2;
using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Behaviors;
using EmpregaNet.Infra.Cache;
using EmpregaNet.Infra.Events;
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
        builder.Services.Configure<SesEmailOptions>(builder.Configuration.GetSection(SesEmailOptions.SectionName));

        var ses = builder.Configuration.GetSection(SesEmailOptions.SectionName).Get<SesEmailOptions>() ?? new SesEmailOptions();
        EnsureSesConfiguredForProduction(builder.Environment, ses);

        if (ses.Enabled && !string.IsNullOrWhiteSpace(ses.FromEmail))
        {
            builder.Services.AddSingleton<IAmazonSimpleEmailServiceV2>(_ => CreateSesClient(ses));
            builder.Services.AddTransient<IEmailSender, SesEmailSender>();
        }
        else if (builder.Environment.IsDevelopment())
            builder.Services.AddTransient<IEmailSender, DevelopmentLogEmailSender>();
        else
            builder.Services.AddTransient<IEmailSender, NoOpEmailSender>();

        builder.Services.AddScoped<IGoogleIdTokenValidator, GoogleIdTokenValidator>();
        builder.Services.AddScoped<IAccountEmailService, AccountEmailService>();
        builder.Services.AddScoped<IJobApplicationEmailService, JobApplicationEmailService>();
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

    private static void EnsureSesConfiguredForProduction(IHostEnvironment env, SesEmailOptions ses)
    {
        if (!env.IsProduction())
            return;

        if (!ses.Enabled)
        {
            throw new InvalidOperationException(
                "Atenção! Em Produção: Ses:Enabled deve ser true para envio real de e-mails. Configure as variáveis de ambiente ou ajuste appsettings.");
        }

        if (string.IsNullOrWhiteSpace(ses.FromEmail))
        {
            throw new InvalidOperationException(
                "Produção: Ses:FromEmail é obrigatório quando Ses:Enabled for true, e tem de ser uma identidade verificada no SES.");
        }

        if (string.IsNullOrWhiteSpace(ses.Region))
        {
            throw new InvalidOperationException(
                "Produção: Ses:Region é obrigatório. A identidade do SES é verificada por região; deixar o SDK " +
                "resolver a região sozinho esconde o erro até o envio falhar em runtime.");
        }
    }

    /// <summary>
    /// Cliente do SES sem credencial explícita: a autenticação usa a cadeia padrão da AWS — IAM role da
    /// instância em produção, perfil ou variáveis <c>AWS_*</c> localmente. Nenhum segredo de e-mail passa
    /// pela configuração da aplicação, que era o caso enquanto o transporte foi SMTP.
    /// </summary>
    private static IAmazonSimpleEmailServiceV2 CreateSesClient(SesEmailOptions ses)
    {
        return string.IsNullOrWhiteSpace(ses.Region)
            ? new AmazonSimpleEmailServiceV2Client()
            : new AmazonSimpleEmailServiceV2Client(RegionEndpoint.GetBySystemName(ses.Region));
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

    /// <summary>
    /// Registra os behaviors do pipeline CQRS. <b>A ordem destas quatro linhas é o mecanismo</b>, não
    /// um detalhe de arrumação.
    /// </summary>
    /// <remarks>
    /// O pipeline é montado de trás para frente (<c>Mediator.RequestHandlerWrapperImpl</c>), logo o
    /// <b>primeiro registado é a camada mais externa</b>. <c>NotificationDispatchBehavior</c> tem de
    /// ficar <b>antes</b> de <c>TransactionBehavior</c> para observar o resultado já commitado; trocar
    /// as duas linhas compila e passa em qualquer teste de caminho feliz, mas passa a enviar e-mail de
    /// dentro da transacção, e um rollback deixaria o candidato informado de uma aprovação que não
    /// existe.
    /// </remarks>
    internal static void AddPipelineBehaviors(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(NotificationDispatchBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
    }

    private static void SetupDependencyInjection(this IServiceCollection services)
    {
        services.AddPipelineBehaviors();

        services.AddScoped<IDomainEventQueue, DomainEventQueue>();
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

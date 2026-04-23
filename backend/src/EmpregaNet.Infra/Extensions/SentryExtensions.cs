using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Infra.Extensions;

/// <summary>
/// Classe de extensão para configurar o Sentry na aplicação ASP.NET Core.
/// </summary>
public static class SentryExtensions
{
    /// <summary>
    /// Adiciona e configura o Sentry para monitoramento de erros e performance.
    /// As configurações são lidas do appsettings.json na seção "Sentry".
    /// </summary>
    /// <param name="builder">O WebApplicationBuilder da aplicação.</param>
    public static void SetupSentryLogging(this WebApplicationBuilder builder)
    {
        // Configura o Sentry para capturar erros e transações
        builder.WebHost.UseSentry(o =>
        {
            builder.Configuration.GetSection("Sentry").Bind(o);
            o.MinimumBreadcrumbLevel = LogLevel.Information;
            o.MinimumEventLevel = LogLevel.Error;
            o.Debug = true;
        });
    }

    /// <summary>
    /// Adiciona o middleware de tracing do Sentry ao pipeline de requisições HTTP.
    /// Deve ser chamado no Configure do Startup ou Program.cs.
    /// </summary>
    /// <param name="app">O WebApplication da aplicação.</param>
    public static void UseSentryTracingMiddleware(this WebApplication app)
    {
        app.UseSentryTracing(); // Para monitoramento de performance (APM)
    }
}
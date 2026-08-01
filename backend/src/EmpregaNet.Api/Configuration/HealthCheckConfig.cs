using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmpregaNet.Api.Configuration;

/// <summary>
/// Publica os dois endpoints de saúde e o formato JSON das respostas.
/// </summary>
/// <remarks>
/// A separação entre <c>live</c> e <c>ready</c> existe porque as duas perguntas têm consequências
/// opostas num orquestrador. <c>live</c> responde “o processo está de pé?” e, se falhar, o
/// contêiner é reiniciado. <c>ready</c> responde “dá para me mandar tráfego?” e, se falhar, a
/// instância apenas sai do balanceador.
///
/// <para>Com um único <c>/health</c> agregando tudo, uma indisponibilidade do Redis derrubaria a
/// verificação e o orquestrador reiniciaria a aplicação em laço, e reiniciar não traz o Redis de
/// volta, e o serviço, que poderia seguir servindo em modo degradado, para de servir.</para>
/// </remarks>
public static class HealthCheckConfig
{
    /// <summary>Marca as verificações que dizem respeito a dependências externas.</summary>
    public const string ReadinessTag = "ready";

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            // Nenhuma verificação: responder já prova que o processo está vivo e atendendo.
            Predicate = _ => false,
            ResponseWriter = WriteJsonResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(ReadinessTag),
            ResponseWriter = WriteJsonResponse
        });

        // Mantido por compatibilidade com o que já aponta para cá.
        app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteJsonResponse });

        return app;
    }

    /// <remarks>
    /// A descrição de cada verificação vai na resposta, mas a exceção não: ela pode conter host,
    /// porta e detalhes de conexão, e este endpoint costuma ficar acessível à rede interna inteira.
    /// </remarks>
    private static async Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 1),
                description = entry.Value.Description
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };
}

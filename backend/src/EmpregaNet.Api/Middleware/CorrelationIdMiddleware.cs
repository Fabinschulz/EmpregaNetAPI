namespace EmpregaNet.Api.Middleware;

/// <summary>
/// Atribui um identificador de correlação por pedido e devolve-o no header de resposta.
/// </summary>
/// <remarks>
/// <c>HttpContext.Items["Correlation-ID"]</c> é lido em quatro pontos, o
/// <c>GlobalExceptionHandler</c>, o <c>OnChallenge</c> e o <c>OnForbidden</c> do JwtBearer, e o
/// <c>OnRejected</c> do rate limiter, mas nada preenchia essa chave.
/// <para>Aceita um <c>X-Correlation-ID</c> enviado pelo cliente, para permitir seguir uma chamada
/// através de vários serviços; caso não venha, gera um. O valor recebido é filtrado e truncado
/// porque acaba em logs e num header de resposta, texto arbitrário de fora não deve ser
/// refletido sem limite.</para>
/// </remarks>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    public const string ItemKey = "Correlation-ID";

    private const int MaxLength = 64;

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Items[ItemKey] = correlationId;
        context.TraceIdentifier = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(incoming))
        {
            return Guid.NewGuid().ToString();
        }

        var sanitized = new string(incoming
            .Trim()
            .Where(c => char.IsLetterOrDigit(c) || c is '-' or '_')
            .Take(MaxLength)
            .ToArray());

        return string.IsNullOrEmpty(sanitized) ? Guid.NewGuid().ToString() : sanitized;
    }
}

using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Infra.Behaviors;

/// <summary>
/// Registra um aviso quando uma requisição do pipeline CQRS passa do limiar de duração.
/// </summary>
/// <remarks>
/// Serve para detectar operações lentas sem instrumentar handler por handler. É o behavior mais
/// externo do pipeline, portanto o tempo medido inclui validação e transação.
///
/// <para><b>Por que lê os claims direto do contexto em vez de usar <c>IHttpCurrentUser</c>:</b>
/// aquele serviço lança quando não há usuário autenticado (<c>GetContextUser()</c> termina em
/// <c>?? throw</c>). Como este behavior roda em <i>toda</i> requisição, inclusive nas anônimas
/// (catálogo público de vagas, e o próprio login), usá-lo transformaria qualquer requisição
/// anônima que passasse do limiar em erro 500, e o login, que faz hash de senha e vai ao banco,
/// é exatamente uma das candidatas naturais a passar. Diagnóstico não pode derrubar o pedido que
/// está a observar: aqui a ausência de usuário é um valor, não uma exceção.</para>
/// </remarks>
/// <typeparam name="TRequest">Tipo da requisição.</typeparam>
/// <typeparam name="TResponse">Tipo da resposta.</typeparam>
public sealed class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int ThresholdInMilliseconds = 500;

    private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PerformanceBehaviour(
        ILogger<PerformanceBehaviour<TRequest, TResponse>> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();

        var elapsedMilliseconds = timer.ElapsedMilliseconds;

        if (elapsedMilliseconds <= ThresholdInMilliseconds)
        {
            return response;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        var userId = user?.FindFirstValue("userId")
                     ?? user?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? "anonimo";

        _logger.LogWarning(
            "Requisição lenta: {RequestName} levou {ElapsedMilliseconds} ms (UserId: {UserId}, CorrelationId: {CorrelationId})",
            typeof(TRequest).Name,
            elapsedMilliseconds,
            userId,
            httpContext?.Items["Correlation-ID"]?.ToString() ?? httpContext?.TraceIdentifier ?? "n/d");

        return response;
    }
}

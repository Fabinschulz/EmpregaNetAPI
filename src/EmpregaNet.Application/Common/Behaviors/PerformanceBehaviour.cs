using EmpregaNet.Application.Service;
using Mediator.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EmpregaNet.Application.Common.Behaviors;

/// <summary>
/// Comportamento de pipeline que monitora a performance das requisições CQRS.
/// 
/// Esse comportamento mede o tempo de execução de cada requisição e, caso ultrapasse
/// um determinado limiar (500 ms), registra um log de advertência contendo informações
/// relevantes como nome da requisição, tempo decorrido, ID e nome do usuário, e detalhes
/// da requisição.
/// 
/// Aplicação típica: Monitoramento de performance e detecção de operações lentas.
/// 
/// <example>
/// Exemplo de injeção:
/// services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
/// </example>
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição.</typeparam>
/// <typeparam name="TResponse">Tipo da resposta.</typeparam>
public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;
    private readonly IHttpCurrentUser _currentUser;

    /// <summary>
    /// Construtor que recebe dependências para logging e contexto do usuário.
    /// </summary>
    /// <param name="logger">Logger para registrar eventos.</param>
    /// <param name="currentUser">Serviço para obter informações do usuário atual.</param>
    public PerformanceBehaviour(ILogger<PerformanceBehaviour<TRequest, TResponse>> logger, IHttpCurrentUser currentUser
    )
    {
        _logger = logger;
        _currentUser = currentUser;
        _timer = new Stopwatch();
    }

    /// <summary>
    /// Manipula a requisição monitorando o tempo de execução.
    /// Se o tempo exceder 500 ms, registra um log de advertência.
    /// </summary>
    /// <param name="request">A requisição sendo processada.</param>
    /// <param name="next">Delegate para o próximo comportamento no pipeline.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>A resposta processada.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();
        var response = await next();
        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        // Limiar configurado: 500 ms
        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var user = _currentUser.GetContextUser();
            var userId = user?.UserToken.Id;
            var userName = user?.UserToken.Username ?? string.Empty;

            _logger.LogWarning("Easymart Long Running Request: {Name} ({ElapsedMilliseconds} ms) {@UserId} {@UserName} {@Request}",
                requestName, elapsedMilliseconds, userId, userName, request
            );
        }

        return response;
    }
}

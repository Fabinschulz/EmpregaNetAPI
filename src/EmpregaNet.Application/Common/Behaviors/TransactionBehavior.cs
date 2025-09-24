using EmpregaNet.Infra.Components.Interfaces;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Common.Behaviors;

/// <summary>
/// Marca uma requisição como transacional, indicando que deve ser executada dentro de uma transação de banco de dados.
/// </summary>
public interface ITransactional { }

/// <summary>
/// Comportamento de pipeline que orquestra a execução de comandos dentro de uma transação atômica.
/// </summary>
/// <remarks>
/// Este comportamento delega a responsabilidade de gerenciar a transação de banco de dados
/// para o <see cref="IUnityOfWork"/>. Ele não inicia ou comita a transação diretamente.
/// Em vez disso, ele utiliza o método <c>ExecuteInTransactionAsync</c> do <c>UnityOfWork</c>
/// para encapsular a execução do comando, garantindo que a operação seja resiliente
/// e atômica.
///
/// Isso mantém a camada de Aplicação agnóstica aos detalhes de implementação do ORM.
/// </remarks>
/// <typeparam name="TRequest">O tipo da requisição, que deve implementar <see cref="ITransactional"/>.</typeparam>
/// <typeparam name="TResponse">O tipo de retorno da requisição.</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ITransactional
{
    private readonly IUnityOfWork _unityOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(IUnityOfWork unityOfWork, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unityOfWork = unityOfWork ?? throw new ArgumentNullException(nameof(unityOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando transação para a requisição {RequestName}", typeof(TRequest).Name);

        var response = await _unityOfWork.ExecuteInTransactionAsync(
            async () =>
            {
                var result = await next();

                _logger.LogInformation("Transação concluída com sucesso para a requisição {RequestName}", typeof(TRequest).Name);

                return result;
            },
            cancellationToken
        );

        return response;
    }
}
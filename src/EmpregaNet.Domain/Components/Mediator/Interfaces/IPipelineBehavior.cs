namespace EmpregaNet.Domain.Components.Mediator.Interfaces
{
    /// <summary>
    /// Delegate que representa a próxima etapa no pipeline de execução de um request.
    /// 
    /// Este delegate encapsula a chamada ao handler real da requisição.
    /// 
    /// É utilizado dentro do pipeline de comportamentos (middlewares) para encadear a execução
    /// até chegar no handler que processará efetivamente o <typeparamref name="TResponse"/>.
    /// 
    /// </summary>
    /// <typeparam name="TResponse">Tipo de resposta esperada pela requisição.</typeparam>
    /// <returns>Uma tarefa que retorna a resposta.</returns>
    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();


    /// <summary>
    /// Interface que define um comportamento no pipeline de execução de uma requisição.
    /// 
    /// Inspirado no comportamento de middlewares, permite adicionar lógica antes e/ou depois da execução do handler real.
    /// 
    /// Exemplos comuns de pipeline behaviors:
    /// - Tratamento de exceções.
    /// - Validação.
    /// - Logging.
    /// - Autorização.
    /// 
    /// A ordem de execução é determinada pela ordem de registro na injeção de dependência.
    /// 
    /// Exemplo de implementação:
    /// public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    /// {
    ///     public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    ///     {
    ///         Console.WriteLine($"Handling {typeof(TRequest).Name}");
    ///         var response = await next();
    ///         Console.WriteLine($"Handled {typeof(TResponse).Name}");
    ///         return response;
    ///     }
    /// }
    /// </summary>
    /// <typeparam name="TRequest">Tipo da requisição.</typeparam>
    /// <typeparam name="TResponse">Tipo da resposta.</typeparam>
    public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {

        /// <summary>
        /// Manipula a requisição, podendo executar lógica antes ou depois da execução do handler.
        /// </summary>
        /// <param name="request">Instância da requisição.</param>
        /// <param name="next">Delegate que representa a próxima etapa no pipeline, geralmente o handler.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação.</param>
        /// <returns>Tarefa que representa a resposta da requisição.</returns>
        Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken);
    }
}
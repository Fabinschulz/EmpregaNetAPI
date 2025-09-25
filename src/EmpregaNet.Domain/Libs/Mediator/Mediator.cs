using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Implementação central do CQRSService responsável por:
/// ✅ Executar comandos e consultas via Send.
/// ✅ Publicar notificações via Publish.
/// ✅ Resolver dinamicamente Handlers e Behaviors via IServiceProvider.
///
/// ⚙️ Características principais:
/// - Suporte a Pipeline Behaviors, permitindo composição de middlewares.
/// - Reflexão para invocar handlers de forma desacoplada.
/// - Utiliza IServiceProvider para resolver dependências.
/// </summary>
internal class Mediator : IMediator
{
    private readonly IServiceProvider _provider;

    public Mediator(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Envia um comando ou consulta, resolvendo o handler apropriado.
    /// Executa também a cadeia de pipeline behaviors.
    /// </summary>
    /// <typeparam name="TResponse">Tipo esperado de resposta.</typeparam>
    /// <param name="request">Instância do comando ou consulta.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resposta processada pelo handler.</returns>
    /// <exception cref="InvalidOperationException">Se o handler não for encontrado.</exception>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = _provider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"Handler not found for {request.GetType().Name}");
        }

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            return (Task<TResponse>)handlerType
                .GetMethod("Handle")!
                .Invoke(handler, new object[] { request, cancellationToken })!;
        };

        var behaviorsType = typeof(IEnumerable<>).MakeGenericType(typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse)));
        var behaviors = ((IEnumerable<object>?)_provider.GetService(behaviorsType))?.Reverse().ToList();

        if (behaviors == null || !behaviors.Any())
        {
            return await handlerDelegate();
        }

        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            var current = behavior;

            handlerDelegate = () =>
            {
                var method = current.GetType().GetMethod("Handle")!;
                return (Task<TResponse>)method.Invoke(current, new object[] { request, next, cancellationToken })!;
            };
        }

        return await handlerDelegate();
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
        var handlers = _provider.GetServices(handlerType);

        // Executa todos os handlers sequencialmente
        foreach (var handler in handlers)
        {
            await (Task)handlerType
                .GetMethod("Handle")!
                .Invoke(handler, new object[] { notification, cancellationToken })!;
        }
    }
}

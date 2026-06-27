using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Implementação central do Mediator responsável por rotear requests para handlers
/// e publicar notificações para todos os handlers registrados.
///
/// Pipeline behaviors são resolvidos via DI e executados na ordem de registro:
/// o primeiro behavior registrado é o mais externo (executa antes dos demais).
///
/// O wrapper por tipo é cacheado estaticamente — sem reflexão no caminho quente.
/// </summary>
internal sealed class Mediator : IMediator
{
    // Cache estático: criado uma vez por tipo de request, compartilhado entre todas as instâncias.
    private static readonly ConcurrentDictionary<Type, object> _handlerCache = new();

    private readonly IServiceProvider _provider;

    public Mediator(IServiceProvider provider) => _provider = provider;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = (RequestHandlerWrapper<TResponse>)_handlerCache.GetOrAdd(
            request.GetType(),
            t => Activator.CreateInstance(
                typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(t, typeof(TResponse)))!
        );

        return wrapper.Handle(request, _provider, cancellationToken);
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        foreach (var handler in _provider.GetServices<INotificationHandler<TNotification>>())
            await handler.Handle(notification, cancellationToken);
    }
}

internal abstract class RequestHandlerWrapper<TResponse>
{
    public abstract Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider provider,
        CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider provider,
        CancellationToken cancellationToken)
    {
        var handler = provider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var behaviors = provider.GetServices<IPipelineBehavior<TRequest, TResponse>>().ToList();

        RequestHandlerDelegate<TResponse> pipeline = () =>
            handler.Handle((TRequest)request, cancellationToken);

        // Itera de trás para frente: primeiro registrado no DI = camada mais externa = executa primeiro.
        for (var i = behaviors.Count - 1; i >= 0; i--)
        {
            var next = pipeline;
            var behavior = behaviors[i];
            pipeline = () => behavior.Handle((TRequest)request, next, cancellationToken);
        }

        return pipeline();
    }
}

using System.Collections.Concurrent;
using System.Reflection;
using EmpregaNet.Application.Abstraction;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Infra.Behaviors;

/// <summary>
/// Publica os eventos de domínio apenas depois que a requisição termina com sucesso.
/// </summary>
/// <remarks>
/// Esta classe depende da ordem do pipeline: ela deve ficar fora da transação para garantir que
/// a publicação aconteça somente após o <c>CommitAsync</c>. Se estiver dentro da transação,
/// um rollback pode deixar notificações enviadas mesmo quando a operação não foi confirmada.
///
/// <para>
/// Se <c>next()</c> lançar exceção, a fila não é drenada e os eventos são descartados junto com
/// a falha da requisição. Também não há falha de resposta por causa de problema na publicação:
/// a operação já foi confirmada, então apenas registramos o erro e seguimos com o resultado.
/// </para>
/// </remarks>
public sealed class NotificationDispatchBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _publishCache = new();
    private static readonly MethodInfo _publishDefinition = typeof(IMediator)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Single(m =>
            m.Name == nameof(IMediator.Publish)
            && m.IsGenericMethodDefinition
            && m.GetGenericArguments().Length == 1
            && m.GetParameters() is [var notification, var token]
            && notification.ParameterType == m.GetGenericArguments()[0]
            && token.ParameterType == typeof(CancellationToken));

    private readonly IDomainEventQueue _queue;
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationDispatchBehavior<TRequest, TResponse>> _logger;

    public NotificationDispatchBehavior(
        IDomainEventQueue queue,
        IMediator mediator,
        ILogger<NotificationDispatchBehavior<TRequest, TResponse>> logger)
    {
        _queue = queue;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        var notifications = _queue.Drain();
        if (notifications.Count == 0)
        {
            return response;
        }

        _logger.LogInformation(
            "Publicando {Count} evento(s) de domínio após a conclusão de {RequestName}.",
            notifications.Count,
            typeof(TRequest).Name);

        foreach (var notification in notifications)
        {
            try
            {
                await PublishAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao publicar {EventName} de {RequestName}. A operação já está confirmada e a resposta segue com êxito.",
                    notification.GetType().Name,
                    typeof(TRequest).Name);
            }
        }

        return response;
    }

    private Task PublishAsync(INotification notification, CancellationToken cancellationToken)
    {
        var publish = _publishCache.GetOrAdd(
            notification.GetType(),
            static type => _publishDefinition.MakeGenericMethod(type));

        return (Task)publish.Invoke(_mediator, [notification, cancellationToken])!;
    }
}

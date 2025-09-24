namespace EmpregaNet.Infra.Components.Interfaces;

/// <summary>
/// Interface genérica para manipuladores de notificações no padrão CQRS.
/// 
/// Responsável por processar eventos que implementam <see cref="INotification"/>.
/// 
/// Cada tipo de notificação pode possuir múltiplos handlers implementando esta interface,
/// permitindo execução de várias ações em resposta a um único evento.
/// </summary>
/// <typeparam name="TNotification"></typeparam>
public interface INotificationHandler<TNotification> where TNotification : INotification
{
    /// <summary>
    /// Manipula a notificação recebida.
    /// </summary>
    /// <param name="notification">Instância da notificação.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <returns>Tarefa representando a operação assíncrona.</returns>
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}
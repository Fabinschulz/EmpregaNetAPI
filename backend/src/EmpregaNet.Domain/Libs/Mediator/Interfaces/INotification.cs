/// <summary>
/// Interface de marcação utilizada para definir objetos de notificação no padrão CQRS.
/// 
/// ✅ Não possui membros — serve apenas para identificar tipos que representam notificações.
/// 
/// Exemplo de uso:
/// public class CreateProductEvent : INotification { }
/// 
/// Todos os tipos que implementarem esta interface podem ser publicados
/// via IMediator.Publish e tratados por INotificationHandler.
/// </summary>
public interface INotification { }
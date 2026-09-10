using EmpregaNet.Application.Abstraction;

namespace EmpregaNet.Infra.Events;

/// <summary>
/// Fila em memória com semântica de conjunto, uma por requisição (registada como <c>Scoped</c>).
/// </summary>
/// <remarks>
/// A identidade usada para descartar repetidos é a <see cref="IIdentifiableNotification.DeduplicationKey"/>
/// quando o evento a fornece, e a igualdade estrutural do próprio evento quando não. É defesa em
/// profundidade sobre as invariantes do domínio, que já recusam repetir a mesma transição: declarar
/// duas vezes o mesmo facto na mesma requisição não pode produzir dois efeitos externos, porque um
/// efeito externo não se desfaz.
///
/// <para>
/// A ordem de entrada é preservada: o primeiro a ser declarado é o primeiro a ser publicado. Importa
/// no encerramento de vaga, onde a ordem dos e-mails segue a ordem das candidaturas afectadas.
/// </para>
///
/// <para>
/// Sem sincronização de propósito: o escopo é a requisição e o pipeline é sequencial — enfileirar e
/// drenar nunca correm em paralelo sobre a mesma instância. Se algum dia um handler passar a
/// paralelizar trabalho que enfileire eventos, isto precisa de mudar, e é por isso que a razão está
/// escrita.
/// </para>
/// </remarks>
public sealed class DomainEventQueue : IDomainEventQueue
{
    private readonly List<INotification> _notifications = [];
    private readonly HashSet<object> _identities = [];

    public void Enqueue(INotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (!_identities.Add(IdentityOf(notification)))
        {
            return;
        }

        _notifications.Add(notification);
    }

    public IReadOnlyList<INotification> Drain()
    {
        if (_notifications.Count == 0)
        {
            return [];
        }

        var drained = _notifications.ToArray();
        _notifications.Clear();
        _identities.Clear();
        return drained;
    }

    /// <summary>
    /// O tipo entra na identidade para dois eventos de tipos diferentes não colidirem por acaso ao
    /// escolherem a mesma chave.
    /// </summary>
    private static object IdentityOf(INotification notification) =>
        notification is IIdentifiableNotification identifiable
            ? (notification.GetType(), identifiable.DeduplicationKey)
            : notification;
}

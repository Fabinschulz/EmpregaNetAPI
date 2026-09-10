using EmpregaNet.Application.Abstraction;

namespace EmpregaNet.Tests.Support;

/// <summary>
/// Fila de eventos de domínio para testes: mesma semântica de conjunto da de produção e um histórico
/// do que foi aceito, imune ao <see cref="Drain"/>.
/// </summary>
/// <remarks>
/// Um mock de <c>IDomainEventQueue</c> verificaria a chamada mas não deixaria inspeccionar o evento —
/// e é na <c>Reason</c> e nos dois estados que está a regra. Como <c>Drain</c> esvazia por contrato,
/// um teste que drenasse perderia o que quer asserir; daí <see cref="Recorded"/> à parte.
///
/// <para>
/// A identidade não é regra reimplementada aqui: é a
/// <see cref="IIdentifiableNotification.DeduplicationKey"/> declarada pelo próprio evento — o mesmo
/// contrato que a fila de produção usa. Quem verifica a implementação de produção é
/// <c>Unit/Behaviors/DomainEventQueueTests</c>.
/// </para>
/// </remarks>
internal sealed class RecordingDomainEventQueue : IDomainEventQueue
{
    private readonly List<INotification> _pending = [];
    private readonly HashSet<object> _identities = [];

    /// <summary>O que a fila aceitou. Repetidos por identidade não aparecem duas vezes.</summary>
    public List<INotification> Recorded { get; } = [];

    public void Enqueue(INotification notification)
    {
        var identity = notification is IIdentifiableNotification identifiable
            ? (notification.GetType(), identifiable.DeduplicationKey)
            : (object)notification;

        if (!_identities.Add(identity))
        {
            return;
        }

        _pending.Add(notification);
        Recorded.Add(notification);
    }

    public IReadOnlyList<INotification> Drain()
    {
        var drained = _pending.ToArray();
        _pending.Clear();
        _identities.Clear();
        return drained;
    }

    /// <summary>Eventos aceites de um tipo concreto, na ordem em que entraram.</summary>
    public IReadOnlyList<T> RecordedOf<T>() where T : INotification => [.. Recorded.OfType<T>()];
}

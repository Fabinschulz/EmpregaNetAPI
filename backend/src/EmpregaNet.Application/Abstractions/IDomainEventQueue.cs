namespace EmpregaNet.Application.Abstraction;

/// <summary>
/// Representa uma notificação cujo identificador lógico não coincide necessariamente com a sua
/// igualdade estrutural.
/// </summary>
/// <remarks>
/// Essa distinção existe para lidar com campos voláteis, como timestamps, IDs gerados em tempo de
/// execução ou outros valores que mudam entre instâncias do mesmo facto. Dois eventos que descrevem o
/// mesmo evento de negócio podem ser diferentes em memória, e a igualdade de record não os reconhece
/// como iguais; sem uma chave de deduplicação, a fila poderia acumular entradas duplicadas e um mesmo
/// e-mail poderia ser enviado mais de uma vez. A propriedade <see cref="DeduplicationKey"/> define o
/// que realmente identifica o facto.
/// </remarks>
public interface IIdentifiableNotification : INotification
{
    string DeduplicationKey { get; }
}

/// <summary>
/// Fila de eventos de domínio acumulados durante uma requisição, para serem publicados somente após o
/// commit da transação.
/// </summary>
public interface IDomainEventQueue
{
    /// <summary>
    /// Registra um evento para publicação caso a operação seja confirmada.
    /// </summary>
    void Enqueue(INotification notification);

    /// <summary>
    /// Remove e retorna todos os eventos acumulados, preservando a ordem de entrada.
    /// </summary>
    IReadOnlyList<INotification> Drain();
}

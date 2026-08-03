namespace EmpregaNet.Tests.Support;

/// <summary>
/// Relógio parado num instante conhecido.
/// </summary>
/// <remarks>
/// Stub local em vez do pacote <c>Microsoft.Extensions.TimeProvider.Testing</c>: os testes só
/// precisam de <see cref="GetUtcNow"/>, e uma dependência inteira para um método não se paga.
/// </remarks>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    private DateTimeOffset _now = now;

    public override DateTimeOffset GetUtcNow() => _now;

    /// <summary>Avança o relógio, para cenários que dependem da passagem do tempo.</summary>
    public void Advance(TimeSpan delta) => _now = _now.Add(delta);
}

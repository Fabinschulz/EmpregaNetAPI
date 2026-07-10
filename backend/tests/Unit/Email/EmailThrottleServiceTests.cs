using EmpregaNet.Infra.Email;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Email;

/// <summary>
/// Objetivo: garantir o teto diário de e-mails por destinatário (anti-abuso de custo de envio).
/// </summary>
public sealed class EmailThrottleServiceTests
{
    [Fact]
    public async Task TryAcquire_DentroDoLimite_DevePermitir()
    {
        var sut = new InMemoryEmailThrottleService(maxPerDay: 5);

        for (var i = 0; i < 5; i++)
        {
            (await sut.TryAcquireAsync("alvo@teste.com")).Should().BeTrue($"envio {i + 1} está dentro do teto");
        }
    }

    [Fact]
    public async Task TryAcquire_AcimaDoLimite_DeveNegar()
    {
        var sut = new InMemoryEmailThrottleService(maxPerDay: 5);

        for (var i = 0; i < 5; i++)
        {
            await sut.TryAcquireAsync("alvo@teste.com");
        }

        (await sut.TryAcquireAsync("alvo@teste.com")).Should().BeFalse("o 6º envio excede o teto diário");
    }

    [Fact]
    public async Task TryAcquire_NormalizaCaseEEspacos_MesmoDestinatario()
    {
        var sut = new InMemoryEmailThrottleService(maxPerDay: 1);

        (await sut.TryAcquireAsync("Alvo@Teste.com")).Should().BeTrue();
        (await sut.TryAcquireAsync("  alvo@teste.com  ")).Should().BeFalse("variações de case/espaço são o mesmo destinatário");
    }

    [Fact]
    public async Task TryAcquire_DestinatariosDiferentes_TemOrcamentosIndependentes()
    {
        var sut = new InMemoryEmailThrottleService(maxPerDay: 1);

        (await sut.TryAcquireAsync("a@teste.com")).Should().BeTrue();
        (await sut.TryAcquireAsync("b@teste.com")).Should().BeTrue("cada destinatário tem o próprio teto");
    }
}

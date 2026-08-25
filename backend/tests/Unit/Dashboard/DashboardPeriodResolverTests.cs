using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.Dashboard;

/// <summary>
/// Fronteiras de período do dashboard.
/// </summary>
/// <remarks>
/// O relógio de referência é <b>03:00 UTC</b> de 19/08/2026, que em Brasília ainda é <b>meia-noite
/// do dia 19</b>. Instante escolhido de propósito: é exatamente onde um cálculo feito em UTC começa
/// a divergir do dia do utilizador, e onde um bug de fuso deixa de ser invisível.
/// </remarks>
public sealed class DashboardPeriodResolverTests
{
    private static readonly DateTimeOffset NowUtc = new(2026, 8, 19, 3, 0, 0, TimeSpan.Zero);

    private static DashboardPeriodResolver CreateSut() => new(new FixedTimeProvider(NowUtc));

    [Fact]
    public void Resolve_Hoje_DeveComecarNaMeiaNoiteDeBrasilia()
    {
        var period = CreateSut().Resolve(DashboardPeriodEnum.Today, null, null);

        period.FromLocal.Should().Be(new DateOnly(2026, 8, 19));
        period.ToLocalInclusive.Should().Be(new DateOnly(2026, 8, 19));

        // Meia-noite de Brasília do dia 19 é 03:00 UTC do dia 19.
        period.Range.FromUtc.Should().Be(new DateTimeOffset(2026, 8, 19, 3, 0, 0, TimeSpan.Zero));
        period.Range.ToUtcExclusive.Should().Be(new DateTimeOffset(2026, 8, 20, 3, 0, 0, TimeSpan.Zero));
        period.DayCount.Should().Be(1);
    }

    [Fact]
    public void Resolve_UltimosSeteDias_DeveIncluirODiaCorrente()
    {
        var period = CreateSut().Resolve(DashboardPeriodEnum.Last7Days, null, null);

        period.FromLocal.Should().Be(new DateOnly(2026, 8, 13));
        period.ToLocalInclusive.Should().Be(new DateOnly(2026, 8, 19));
        period.DayCount.Should().Be(7);
    }

    [Fact]
    public void Resolve_EsteAno_DeveComecarEmPrimeiroDeJaneiro()
    {
        var period = CreateSut().Resolve(DashboardPeriodEnum.ThisYear, null, null);

        period.FromLocal.Should().Be(new DateOnly(2026, 1, 1));
        period.ToLocalInclusive.Should().Be(new DateOnly(2026, 8, 19));
    }

    [Fact]
    public void Resolve_DeveProduzirIntervaloSemiabertoSemBuraco()
    {
        var period = CreateSut().Resolve(DashboardPeriodEnum.Last30Days, null, null);
        var previous = period.Range.Previous();

        // O fim exclusivo do período anterior é exatamente o início do atual: sem sobreposição e
        // sem o segundo perdido do clássico "até 23:59:59".
        previous.ToUtcExclusive.Should().Be(period.Range.FromUtc);
        previous.Duration.Should().Be(period.Range.Duration);
    }

    [Fact]
    public void Resolve_Personalizado_DeveRespeitarAsDatasInformadas()
    {
        var period = CreateSut().Resolve(
            DashboardPeriodEnum.Custom,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31));

        period.DayCount.Should().Be(31);
        period.Range.FromUtc.Should().Be(new DateTimeOffset(2026, 7, 1, 3, 0, 0, TimeSpan.Zero));
        period.Range.ToUtcExclusive.Should().Be(new DateTimeOffset(2026, 8, 1, 3, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Resolve_PersonalizadoSemDatas_DeveFalhar()
    {
        var act = () => CreateSut().Resolve(DashboardPeriodEnum.Custom, null, null);

        act.Should().Throw<ValidationAppException>()
            .Which.Code.Should().Be(DomainErrorEnum.INVALID_QUERY_FILTER);
    }

    [Fact]
    public void Resolve_PersonalizadoInvertido_DeveFalhar()
    {
        var act = () => CreateSut().Resolve(
            DashboardPeriodEnum.Custom,
            new DateOnly(2026, 8, 30),
            new DateOnly(2026, 8, 1));

        act.Should().Throw<ValidationAppException>();
    }

    [Fact]
    public void Resolve_PersonalizadoAcimaDoTeto_DeveFalhar()
    {
        var from = new DateOnly(2025, 1, 1);
        var to = from.AddDays(DashboardPeriodResolver.MaxCustomRangeDays);

        var act = () => CreateSut().Resolve(DashboardPeriodEnum.Custom, from, to);

        act.Should().Throw<ValidationAppException>();
    }

    [Theory]
    [InlineData(DashboardPeriodEnum.Today, DashboardGranularityEnum.Daily)]
    [InlineData(DashboardPeriodEnum.Last30Days, DashboardGranularityEnum.Daily)]
    [InlineData(DashboardPeriodEnum.Last90Days, DashboardGranularityEnum.Weekly)]
    [InlineData(DashboardPeriodEnum.ThisYear, DashboardGranularityEnum.Monthly)]
    public void ResolveGranularity_SemPedido_DeveEscolherPeloTamanhoDaJanela(
        DashboardPeriodEnum requested,
        DashboardGranularityEnum expected)
    {
        var sut = CreateSut();
        var period = sut.Resolve(requested, null, null);

        sut.ResolveGranularity(period, null).Should().Be(expected);
    }

    [Fact]
    public void ResolveGranularity_ComPedidoExplicito_DeveRespeitarOPedido()
    {
        var sut = CreateSut();
        var period = sut.Resolve(DashboardPeriodEnum.ThisYear, null, null);

        sut.ResolveGranularity(period, DashboardGranularityEnum.Daily)
            .Should().Be(DashboardGranularityEnum.Daily);
    }
}

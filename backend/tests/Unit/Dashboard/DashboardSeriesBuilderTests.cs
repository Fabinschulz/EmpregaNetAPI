using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Enums;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.Dashboard;

/// <summary>
/// Dobra dos baldes diários em séries de gráfico.
/// </summary>
public sealed class DashboardSeriesBuilderTests
{
    private static DashboardResolvedPeriod Period(DateOnly from, DateOnly to)
    {
        var offset = TimeSpan.FromHours(-3);
        var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), offset).ToUniversalTime();
        var toUtc = new DateTimeOffset(to.AddDays(1).ToDateTime(TimeOnly.MinValue), offset).ToUniversalTime();

        return new DashboardResolvedPeriod(new DashboardDateRange(fromUtc, toUtc), offset, from, to);
    }

    [Fact]
    public void Build_Diario_DevePreencherDiasSemDadoComZero()
    {
        var period = Period(new DateOnly(2026, 8, 17), new DateOnly(2026, 8, 21));
        var points = new List<DashboardDailyPoint>
        {
            new(new DateOnly(2026, 8, 17), 3),
            new(new DateOnly(2026, 8, 20), 5)
        };

        var series = DashboardSeriesBuilder.Build(
            "jobs", "Vagas", points, period, DashboardGranularityEnum.Daily);

        // Cinco baldes, não dois: sem o zero explícito o gráfico ligaria 17/08 a 20/08 com uma reta
        // que esconde os três dias sem publicação.
        series.Points.Should().HaveCount(5);
        series.Points.Select(p => p.Value).Should().Equal(3, 0, 0, 5, 0);
        series.Total.Should().Be(8);
    }

    [Fact]
    public void Build_Diario_DeveRotularEmFormatoBrasileiro()
    {
        var period = Period(new DateOnly(2026, 8, 17), new DateOnly(2026, 8, 17));

        var series = DashboardSeriesBuilder.Build(
            "jobs", "Vagas", [], period, DashboardGranularityEnum.Daily);

        series.Points.Should().ContainSingle();
        series.Points[0].Date.Should().Be("2026-08-17");
        series.Points[0].Label.Should().Be("17/08");
    }

    [Fact]
    public void Build_Semanal_DeveRecortarOPrimeiroEUltimoBaldeNoPeriodo()
    {
        // 19/08/2026 é quarta-feira: a primeira semana do período tem 5 dias, não 7.
        var period = Period(new DateOnly(2026, 8, 19), new DateOnly(2026, 9, 1));
        var points = new List<DashboardDailyPoint>
        {
            new(new DateOnly(2026, 8, 19), 1),
            new(new DateOnly(2026, 8, 23), 2),
            new(new DateOnly(2026, 8, 24), 4),
            new(new DateOnly(2026, 9, 1), 7)
        };

        var series = DashboardSeriesBuilder.Build(
            "applications", "Candidaturas", points, period, DashboardGranularityEnum.Weekly);

        // Baldes: 19–23/08 (recortado), 24–30/08 (completo), 31/08–01/09 (recortado).
        series.Points.Select(p => p.Date).Should().Equal("2026-08-19", "2026-08-24", "2026-08-31");
        series.Points.Select(p => p.Value).Should().Equal(3, 4, 7);
        series.Total.Should().Be(14);
    }

    [Fact]
    public void Build_Mensal_DeveAgruparPorMesCalendario()
    {
        var period = Period(new DateOnly(2026, 6, 15), new DateOnly(2026, 8, 10));
        var points = new List<DashboardDailyPoint>
        {
            new(new DateOnly(2026, 6, 20), 2),
            new(new DateOnly(2026, 7, 1), 3),
            new(new DateOnly(2026, 7, 31), 4),
            new(new DateOnly(2026, 8, 10), 5)
        };

        var series = DashboardSeriesBuilder.Build(
            "candidates", "Candidatos", points, period, DashboardGranularityEnum.Monthly);

        series.Points.Select(p => p.Date).Should().Equal("2026-06-15", "2026-07-01", "2026-08-01");
        series.Points.Select(p => p.Value).Should().Equal(2, 7, 5);
        series.Points[0].Label.Should().Contain("jun");
    }

    [Fact]
    public void Build_SemDadoNenhum_DeveDevolverBaldesZerados()
    {
        var period = Period(new DateOnly(2026, 8, 17), new DateOnly(2026, 8, 19));

        var series = DashboardSeriesBuilder.Build(
            "users", "Usuários", [], period, DashboardGranularityEnum.Daily);

        series.Points.Should().HaveCount(3);
        series.Total.Should().Be(0);
        series.Points.Should().OnlyContain(p => p.Value == 0);
    }

    [Fact]
    public void Build_DeveIgnorarPontosForaDoPeriodo()
    {
        var period = Period(new DateOnly(2026, 8, 18), new DateOnly(2026, 8, 19));
        var points = new List<DashboardDailyPoint>
        {
            new(new DateOnly(2026, 8, 10), 99),
            new(new DateOnly(2026, 8, 18), 1)
        };

        var series = DashboardSeriesBuilder.Build(
            "jobs", "Vagas", points, period, DashboardGranularityEnum.Daily);

        series.Total.Should().Be(1);
    }
}

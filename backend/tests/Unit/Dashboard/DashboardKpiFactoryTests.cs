using EmpregaNet.Application.Dashboard.UseCase;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.Dashboard;

/// <summary>
/// Regra de comparação dos cartões — o ponto onde uma percentagem inventada entraria na tela.
/// </summary>
public sealed class DashboardKpiFactoryTests
{
    [Fact]
    public void Comparable_ComBaseValida_DeveCalcularVariacaoETendencia()
    {
        var kpi = DashboardKpiFactory.Comparable("newApplications", "Candidaturas", 120m, 100m, "hint");

        kpi.ChangePercent.Should().Be(20m);
        kpi.Trend.Should().Be("up");
        kpi.IsPeriodScoped.Should().BeTrue();
    }

    [Fact]
    public void Comparable_ComQuedaDeveMarcarTendenciaDeBaixa()
    {
        var kpi = DashboardKpiFactory.Comparable("newJobs", "Vagas", 80m, 100m, "hint");

        kpi.ChangePercent.Should().Be(-20m);
        kpi.Trend.Should().Be("down");
    }

    [Fact]
    public void Comparable_ComPeriodoAnteriorZerado_NaoDeveInventarPercentagem()
    {
        var kpi = DashboardKpiFactory.Comparable("newJobs", "Vagas", 7m, 0m, "hint");

        // Sem base não há percentagem — mas a direção é conhecida e a tela pode dizer "novo".
        kpi.ChangePercent.Should().BeNull();
        kpi.PreviousValue.Should().Be(0m);
        kpi.Trend.Should().Be("up");
    }

    [Fact]
    public void Comparable_ComAmbosZerados_DeveFicarEstavel()
    {
        var kpi = DashboardKpiFactory.Comparable("newJobs", "Vagas", 0m, 0m, "hint");

        kpi.ChangePercent.Should().BeNull();
        kpi.Trend.Should().Be("flat");
    }

    [Fact]
    public void Comparable_SemPeriodoAnterior_NaoDeveExibirTendencia()
    {
        var kpi = DashboardKpiFactory.Comparable("conversionRate", "Conversão", 12m, null, "hint", unit: "percent");

        kpi.PreviousValue.Should().BeNull();
        kpi.ChangePercent.Should().BeNull();
        kpi.Trend.Should().Be("none");
        kpi.Unit.Should().Be("percent");
    }

    [Fact]
    public void Snapshot_DeveMarcarQueNaoDependeDoPeriodo()
    {
        var kpi = DashboardKpiFactory.Snapshot("activeJobs", "Vagas ativas", 42m, "hint");

        kpi.IsPeriodScoped.Should().BeFalse();
        kpi.PreviousValue.Should().BeNull();
        kpi.Trend.Should().Be("none");
    }

    [Fact]
    public void ChangePercent_DeveArredondarComUmaCasa()
    {
        DashboardKpiFactory.ChangePercent(1284m, 1142m).Should().Be(12.4m);
    }
}

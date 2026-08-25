using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Enums;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.Dashboard;

/// <summary>
/// Montagem das distribuições: percentagens, cauda longa e soma que não fecha.
/// </summary>
public sealed class DashboardBreakdownFactoryTests
{
    private static DashboardEnumCount<JobAreaEnum> Area(JobAreaEnum area, int count) => new(area, count);

    [Fact]
    public void Create_DeveOrdenarPorVolumeECalcularPercentagem()
    {
        var breakdown = DashboardBreakdownFactory.Create<JobAreaEnum>(
        [
            Area(JobAreaEnum.Logistica, 20),
            Area(JobAreaEnum.Producao, 30)
        ]);

        breakdown.Items.Select(i => i.Key).Should().Equal("Producao", "Logistica");
        breakdown.Items[0].Share.Should().Be(60m);
        breakdown.Items[1].Share.Should().Be(40m);
        breakdown.Total.Should().Be(50);
        breakdown.Categorized.Should().Be(50);
        breakdown.Note.Should().BeNull();
    }

    [Fact]
    public void Create_DeveUsarADescricaoComoRotulo()
    {
        var breakdown = DashboardBreakdownFactory.Create<JobAreaEnum>([Area(JobAreaEnum.Ti, 1)]);

        breakdown.Items[0].Label.Should().Be("Tecnologia da Informação");
    }

    [Fact]
    public void Create_DeveDescartarOMembroNeutro()
    {
        var breakdown = DashboardBreakdownFactory.Create(
            [Area(JobAreaEnum.NaoSelecionado, 7), Area(JobAreaEnum.Producao, 3)],
            neutral: JobAreaEnum.NaoSelecionado);

        breakdown.Items.Should().ContainSingle().Which.Key.Should().Be("Producao");
        breakdown.Categorized.Should().Be(3);
    }

    [Fact]
    public void Create_DeveDescartarCategoriasZeradas()
    {
        var breakdown = DashboardBreakdownFactory.Create<JobAreaEnum>(
            [Area(JobAreaEnum.Producao, 0), Area(JobAreaEnum.Logistica, 4)]);

        breakdown.Items.Should().ContainSingle().Which.Key.Should().Be("Logistica");
    }

    [Fact]
    public void Create_ComCaudaLonga_DeveAgregarEmOutras()
    {
        var counts = new[]
        {
            Area(JobAreaEnum.Producao, 50),
            Area(JobAreaEnum.Logistica, 30),
            Area(JobAreaEnum.Manutencao, 10),
            Area(JobAreaEnum.Qualidade, 6),
            Area(JobAreaEnum.Comercial, 4)
        };

        var breakdown = DashboardBreakdownFactory.Create<JobAreaEnum>(counts, topN: 3);

        breakdown.Items.Should().HaveCount(4);
        breakdown.Items[^1].Key.Should().Be(DashboardBreakdownFactory.OthersKey);
        breakdown.Items[^1].Value.Should().Be(10);

        // A cauda agregada mantém a soma: as fatias continuam a fechar em 100%.
        breakdown.Items.Sum(i => i.Value).Should().Be(breakdown.Categorized);
        breakdown.Items.Sum(i => i.Share).Should().Be(100m);
    }

    [Fact]
    public void Create_QuandoOUniversoEMaiorQueAsFatias_DeveExplicarADiferenca()
    {
        var breakdown = DashboardBreakdownFactory.Create(
            [new DashboardEnumCount<UF>(UF.SP, 30), new DashboardEnumCount<UF>(UF.MG, 20)],
            total: 62,
            neutral: UF.NaoSelecionado,
            uncategorizedNote: "{0} candidatos sem UF no cadastro ficam fora desta distribuição.");

        breakdown.Total.Should().Be(62);
        breakdown.Categorized.Should().Be(50);
        breakdown.Note.Should().Be("12 candidatos sem UF no cadastro ficam fora desta distribuição.");
    }

    [Fact]
    public void Create_SemDados_DeveDevolverDistribuicaoVazia()
    {
        var breakdown = DashboardBreakdownFactory.Create<JobAreaEnum>([]);

        breakdown.Items.Should().BeEmpty();
        breakdown.Total.Should().Be(0);
        breakdown.Note.Should().BeNull();
    }
}

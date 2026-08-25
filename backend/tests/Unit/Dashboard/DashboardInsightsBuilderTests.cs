using EmpregaNet.Application.Dashboard.UseCase;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.Dashboard;

/// <summary>
/// Composição dos insights: o que se diz, e sobretudo o que não se diz.
/// </summary>
public sealed class DashboardInsightsBuilderTests
{
    private static DashboardInsightsInput Empty => new(
        ActiveJobs: 0,
        StaleJobs: 0,
        StaleDays: 30,
        JobsWithoutApplications: 0,
        Applications: 0,
        PreviousApplications: 0,
        NewCandidates: 0,
        PreviousNewCandidates: 0,
        TopApplicationArea: null,
        TopApplicationAreaCount: 0,
        CategorizedApplications: 0,
        TopJobTitle: null,
        TopJobApplications: 0,
        AverageApplicationsPerJob: 0);

    [Fact]
    public void Build_SemDados_NaoDeveInventarLeitura()
    {
        DashboardInsightsBuilder.Build(Empty).Should().BeEmpty();
    }

    [Fact]
    public void Build_ComVagasEstagnadas_DeveAlertar()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with { StaleJobs = 5, StaleDays = 30 });

        insights.Should().ContainSingle(i => i.Code == "staleJobs")
            .Which.Message.Should().Be("5 vagas ativas estão há mais de 30 dias sem novas candidaturas.");
    }

    [Fact]
    public void Build_ComUmaVagaEstagnada_DeveUsarSingular()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with { StaleJobs = 1 });

        insights.Single(i => i.Code == "staleJobs").Message
            .Should().StartWith("1 vaga ativa está");
    }

    [Fact]
    public void Build_AreaConcentrada_DeveRelatarPercentagem()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            TopApplicationArea = "Tecnologia da Informação",
            TopApplicationAreaCount = 38,
            CategorizedApplications = 100
        });

        insights.Should().ContainSingle(i => i.Code == "areaConcentration")
            .Which.Message.Should().Contain("38%").And.Contain("Tecnologia da Informação");
    }

    [Fact]
    public void Build_AreaAbaixoDoLimiar_NaoDeveRelatar()
    {
        // 12% entre 16 áreas é distribuição normal, não concentração.
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            TopApplicationArea = "Logística",
            TopApplicationAreaCount = 12,
            CategorizedApplications = 100
        });

        insights.Should().NotContain(i => i.Code == "areaConcentration");
    }

    [Fact]
    public void Build_VagaMuitoAcimaDaMedia_DeveDestacar()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            TopJobTitle = "Desenvolvedor Full Stack",
            TopJobApplications = 128,
            AverageApplicationsPerJob = 90
        });

        insights.Should().ContainSingle(i => i.Code == "topJobAboveAverage")
            .Which.Message.Should().Contain("42%").And.Contain("Desenvolvedor Full Stack");
    }

    [Fact]
    public void Build_VagaDentroDoRuido_NaoDeveDestacar()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            TopJobTitle = "Auxiliar de Produção",
            TopJobApplications = 92,
            AverageApplicationsPerJob = 90
        });

        insights.Should().NotContain(i => i.Code == "topJobAboveAverage");
    }

    [Fact]
    public void Build_SemMedia_NaoDeveCompararComMedia()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            TopJobTitle = "Operador de Empilhadeira",
            TopJobApplications = 10,
            AverageApplicationsPerJob = 0
        });

        insights.Should().BeEmpty();
    }

    [Fact]
    public void Build_ComVagasEstagnadas_DeveRelatarProporcaoQuandoHaBase()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with { StaleJobs = 5, ActiveJobs = 20 });

        insights.Single(i => i.Code == "staleJobs").Message
            .Should().EndWith("São 25% das vagas abertas.");
    }

    [Fact]
    public void Build_VagasEstagnadasNaMaiorParteDaCarteira_DeveElevarSeveridade()
    {
        var alta = DashboardInsightsBuilder.Build(Empty with { StaleJobs = 8, ActiveJobs = 20 });
        var media = DashboardInsightsBuilder.Build(Empty with { StaleJobs = 2, ActiveJobs = 20 });

        alta.Single(i => i.Code == "staleJobs").Severity.Should().Be("high");
        media.Single(i => i.Code == "staleJobs").Severity.Should().Be("medium");
    }

    [Fact]
    public void Build_VagasSemCandidaturaAcimaDoLimiar_DeveAlertar()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            ActiveJobs = 10,
            JobsWithoutApplications = 3
        });

        var insight = insights.Single(i => i.Code == "jobsWithoutApplications");
        insight.Category.Should().Be("attention");
        insight.Message.Should().Contain("30%");
    }

    [Fact]
    public void Build_PoucasVagasSemCandidatura_NaoDeveAlertar()
    {
        // Uma vaga nova sem candidatura numa carteira de 40 é rotina, não problema.
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            ActiveJobs = 40,
            JobsWithoutApplications = 1
        });

        insights.Should().NotContain(i => i.Code == "jobsWithoutApplications");
    }

    [Fact]
    public void Build_CandidaturasEmAlta_DeveEntrarComoCrescimento()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            Applications = 1842,
            PreviousApplications = 1558
        });

        var insight = insights.Single(i => i.Code == "applicationsGrowth");
        insight.Category.Should().Be("growth");
        insight.Tone.Should().Be("positive");
        insight.Title.Should().Contain("18,2%");
    }

    [Fact]
    public void Build_CandidaturasEmQueda_DeveEntrarComoAtencao()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            Applications = 700,
            PreviousApplications = 1000
        });

        var insight = insights.Single(i => i.Code == "applicationsDrop");
        insight.Category.Should().Be("attention");
        insight.Tone.Should().Be("negative");
        insight.Severity.Should().Be("high");
    }

    [Fact]
    public void Build_VariacaoDentroDoRuido_NaoDeveRelatar()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            Applications = 104,
            PreviousApplications = 100
        });

        insights.Should().BeEmpty();
    }

    [Fact]
    public void Build_SemPeriodoAnterior_NaoDeveInventarVariacao()
    {
        // Base zero não é crescimento de 100%: é ausência de base.
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            Applications = 120,
            PreviousApplications = 0
        });

        insights.Should().NotContain(i => i.Category == "growth");
    }

    [Fact]
    public void Build_TodaLeitura_DeveTerCategoriaTituloESeveridade()
    {
        var insights = DashboardInsightsBuilder.Build(Empty with
        {
            ActiveJobs = 20,
            StaleJobs = 5,
            JobsWithoutApplications = 6,
            Applications = 1842,
            PreviousApplications = 1558,
            NewCandidates = 300,
            PreviousNewCandidates = 200,
            TopApplicationArea = "Tecnologia da Informação",
            TopApplicationAreaCount = 38,
            CategorizedApplications = 100,
            TopJobTitle = "Desenvolvedor Full Stack",
            TopJobApplications = 128,
            AverageApplicationsPerJob = 90
        });

        insights.Should().OnlyContain(i =>
            i.Category.Length > 0 && i.Title.Length > 0 && i.Severity.Length > 0 && i.Message.Length > 0);

        insights.Select(i => i.Category).Should()
            .Contain(["attention", "growth", "highlight", "behavior"]);
    }
}

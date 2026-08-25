using EmpregaNet.Application.Dashboard;
using EmpregaNet.Application.Dashboard.Queries;
using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Application.Dashboard.ViewModel;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.Dashboard;

/// <summary>
/// Decisões do handler da visão geral: quais cartões existem em cada escopo e como o funil é montado.
/// </summary>
public sealed class GetDashboardOverviewHandlerTests
{
    private static readonly DateTimeOffset From = new(2026, 7, 20, 3, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To = new(2026, 8, 19, 3, 0, 0, TimeSpan.Zero);

    private readonly Mock<IDashboardAnalyticsRepository> _repository = new();
    private readonly Mock<IDashboardContextFactory> _contextFactory = new();

    private GetDashboardOverviewHandler CreateSut() =>
        new(_repository.Object, _contextFactory.Object, NullLogger<GetDashboardOverviewHandler>.Instance);

    private void SetupScope(DashboardScope scope)
    {
        var period = new DashboardResolvedPeriod(
            new DashboardDateRange(From, To),
            TimeSpan.FromHours(-3),
            new DateOnly(2026, 7, 20),
            new DateOnly(2026, 8, 18));

        var filter = new DashboardFilter(scope, period.Range, period.LocalOffset);
        var context = new DashboardQueryContext(filter, period, new DashboardResolvedScope(scope, "Acme"));

        _contextFactory
            .Setup(x => x.CreateAsync(It.IsAny<DashboardFilterInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);

        _contextFactory
            .Setup(x => x.CreateMeta(
                It.IsAny<DashboardQueryContext>(),
                It.IsAny<DashboardFilterInput>(),
                It.IsAny<IReadOnlyList<DashboardUnavailableMetricViewModel>>()))
            .Returns(new DashboardMetaViewModel
            {
                Period = "Last30Days",
                PeriodLabel = "Últimos 30 dias",
                From = "20/07/2026",
                To = "18/08/2026",
                FromUtc = From.ToString("O"),
                ToUtcExclusive = To.ToString("O"),
                Days = 30,
                PreviousFrom = "20/06/2026",
                PreviousTo = "19/07/2026",
                GeneratedAt = "19/08/2026 00:00:00",
                Scope = new DashboardScopeViewModel(scope.IsPlatformWide ? "platform" : "company", scope.CompanyId, "Acme")
            });
    }

    private void SetupData(
        DashboardTotals totals,
        DashboardPeriodCounters current,
        DashboardPeriodCounters previous,
        IReadOnlyList<DashboardStatusComparison> byStatus)
    {
        _repository
            .Setup(x => x.GetTotalsAsync(It.IsAny<DashboardFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(totals);

        _repository
            .Setup(x => x.GetComparedCountersAsync(It.IsAny<DashboardFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardComparedCounters(current, previous));

        _repository
            .Setup(x => x.GetApplicationsByStatusAsync(It.IsAny<DashboardFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(byStatus);
    }

    private static DashboardTotals PlatformTotals => new(ActiveJobs: 42, CompaniesWithActiveJobs: 8);

    private static DashboardTotals CompanyTotals => new(ActiveJobs: 5, CompaniesWithActiveJobs: 1);

    private static DashboardPeriodCounters Counters(int jobs, int applications, int candidates)
        => new(NewJobs: jobs, NewApplications: applications, NewCandidates: candidates);

    [Fact]
    public async Task Handle_NaVisaoDePlataforma_DeveDevolverSeisIndicadoresDeOperacao()
    {
        SetupScope(DashboardScope.Platform);
        SetupData(PlatformTotals, Counters(10, 200, 50), Counters(8, 150, 40), []);

        var result = await CreateSut().Handle(new GetDashboardOverviewQuery(new DashboardFilterInput()), default);

        result.Kpis.Select(k => k.Key).Should().BeEquivalentTo(
            ["activeJobs", "newApplications", "newCandidates", "newJobs", "conversionRate", "activeCompanies"]);
    }

    [Fact]
    public async Task Handle_NaoDeveDevolverIndicadoresAdministrativos()
    {
        SetupScope(DashboardScope.Platform);
        SetupData(PlatformTotals, Counters(10, 200, 50), Counters(8, 150, 40), []);

        var result = await CreateSut().Handle(new GetDashboardOverviewQuery(new DashboardFilterInput()), default);

        // Estes números continuam calculáveis e chegam do repositório; ficaram fora do painel por
        // não sustentarem decisão — acumulados sem comparação, administração de sistema, ou
        // aproximações de baixa qualidade. Este teste é o que impede que voltem por inércia.
        result.Kpis.Select(k => k.Key).Should().NotContain(
            ["totalCandidates", "totalCompanies", "totalUsers", "newUsers", "newCompanies", "unconfirmedUsers", "closedJobs"]);
    }

    [Fact]
    public async Task Handle_NoEscopoDeEmpresa_NaoDeveExporNumerosDaPlataforma()
    {
        SetupScope(new DashboardScope(7));
        SetupData(CompanyTotals, Counters(3, 40, 12), Counters(2, 30, 10), []);

        var result = await CreateSut().Handle(new GetDashboardOverviewQuery(new DashboardFilterInput()), default);

        // "Empresas com vagas ativas" é número da plataforma; num painel de empresa seria dado de
        // terceiros.
        result.Kpis.Select(k => k.Key).Should().BeEquivalentTo(
            ["activeJobs", "newApplications", "newCandidates", "newJobs", "conversionRate"]);
    }

    [Fact]
    public async Task Handle_DeveContarConcluidasComoTendoChegadoAAprovacao()
    {
        SetupScope(DashboardScope.Platform);
        SetupData(
            PlatformTotals,
            Counters(10, 100, 50),
            Counters(8, 80, 40),
            [
                new DashboardStatusComparison(ApplicationStatusEnum.Processing, 60, 50),
                new DashboardStatusComparison(ApplicationStatusEnum.Approved, 20, 10),
                new DashboardStatusComparison(ApplicationStatusEnum.Finished, 10, 5),
                new DashboardStatusComparison(ApplicationStatusEnum.Rejected, 10, 15)
            ]);

        var result = await CreateSut().Handle(new GetDashboardOverviewQuery(new DashboardFilterInput()), default);

        var approved = result.Funnel.Stages.Single(s => s.Key == "approved");

        // 20 aprovadas + 10 concluídas: sem somar, a etapa encolheria justamente quando o processo
        // avança, porque o status guarda apenas o estado atual.
        approved.Value.Should().Be(30);
        approved.ShareOfPrevious.Should().Be(30m);

        result.Funnel.ConversionRate.Should().Be(30m);
        result.Funnel.Stages.Single(s => s.Key == "finished").Value.Should().Be(10);
    }

    [Fact]
    public async Task Handle_DeveCompararATaxaDeAprovacaoComOPeriodoAnterior()
    {
        SetupScope(DashboardScope.Platform);
        SetupData(
            PlatformTotals,
            Counters(10, 100, 50),
            Counters(8, 80, 40),
            [
                new DashboardStatusComparison(ApplicationStatusEnum.Processing, 70, 60),
                new DashboardStatusComparison(ApplicationStatusEnum.Approved, 30, 20)
            ]);

        var result = await CreateSut().Handle(new GetDashboardOverviewQuery(new DashboardFilterInput()), default);
        var conversion = result.Kpis.Single(k => k.Key == "conversionRate");

        // Atual: 30/100 = 30%. Anterior: 20/80 = 25%.
        conversion.Value.Should().Be(30m);
        conversion.PreviousValue.Should().Be(25m);
        conversion.Unit.Should().Be("percent");
        conversion.Trend.Should().Be("up");
    }

    [Fact]
    public async Task Handle_SemCandidaturas_NaoDeveInventarTaxaDeConversao()
    {
        SetupScope(DashboardScope.Platform);
        SetupData(PlatformTotals, Counters(1, 0, 0), Counters(0, 0, 0), []);

        var result = await CreateSut().Handle(new GetDashboardOverviewQuery(new DashboardFilterInput()), default);

        result.Funnel.ConversionRate.Should().BeNull();
        result.Funnel.Stages.Single(s => s.Key == "approved").ShareOfPrevious.Should().BeNull();

        // O cartão também não pode dizer "0%": sem candidatura a taxa é uma divisão sem
        // denominador, e zero seria lido como "ninguém foi aprovado".
        result.Kpis.Single(k => k.Key == "conversionRate").Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_FunilDeveComecarNaCandidatura()
    {
        SetupScope(DashboardScope.Platform);
        SetupData(PlatformTotals, Counters(10, 200, 50), Counters(8, 150, 40), []);

        var result = await CreateSut().Handle(new GetDashboardOverviewQuery(new DashboardFilterInput()), default);

        // "Vagas publicadas" não é etapa de funil: conta outra unidade, não produz conversão e
        // obrigava a tela a rotulá-la "contexto". O número vive no cartão, com comparação.
        result.Funnel.Stages.Select(s => s.Key).Should()
            .BeEquivalentTo(["applications", "approved", "finished"], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Handle_VagasAtivas_NaoDeveSerMarcadoComoDependenteDoPeriodo()
    {
        SetupScope(DashboardScope.Platform);
        SetupData(PlatformTotals, Counters(10, 100, 50), Counters(8, 80, 40), []);

        var result = await CreateSut().Handle(new GetDashboardOverviewQuery(new DashboardFilterInput()), default);

        // IsActive não tem histórico: o cartão declara que ignora o filtro de período, para a tela
        // poder dizê-lo em vez de deixar o utilizador supor o contrário.
        result.Kpis.Single(k => k.Key == "activeJobs").IsPeriodScoped.Should().BeFalse();
        result.Kpis.Single(k => k.Key == "newJobs").IsPeriodScoped.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DeveConsultarCadaAgregadoUmaVezSo()
    {
        SetupScope(DashboardScope.Platform);
        SetupData(PlatformTotals, Counters(10, 100, 50), Counters(8, 80, 40), []);

        await CreateSut().Handle(new GetDashboardOverviewQuery(new DashboardFilterInput()), default);

        // A comparação com o período anterior sai da mesma chamada: duas seriam duas varreduras
        // para produzir os mesmos números.
        _repository.Verify(
            x => x.GetComparedCountersAsync(It.IsAny<DashboardFilter>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _repository.Verify(
            x => x.GetTotalsAsync(It.IsAny<DashboardFilter>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _repository.Verify(
            x => x.GetApplicationsByStatusAsync(It.IsAny<DashboardFilter>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

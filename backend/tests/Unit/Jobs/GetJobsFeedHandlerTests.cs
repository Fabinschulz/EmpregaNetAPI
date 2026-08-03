using EmpregaNet.Application.Jobs.Queries;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using EmpregaNet.Tests.Support;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.Jobs;

/// <summary>
/// Cobre a montagem do filtro e as decisões do handler. A <b>execução</b> do SQL (full-text,
/// sobreposição de arrays, ts_rank) não é exercitada aqui nem na suíte de integração - o provider
/// InMemory não implementa nada disso. Gap registado em <c>docs/features/emp-feed-vagas/spec.md</c>.
/// </summary>
public sealed class GetJobsFeedHandlerTests
{
    private readonly Mock<IJobRepository> _repo = new();
    private readonly FixedTimeProvider _clock = new(new DateTimeOffset(2026, 8, 2, 2, 30, 0, TimeSpan.Zero));

    private GetJobsFeedHandler CreateSut() =>
        new(_repo.Object, NullLogger<GetJobsFeedHandler>.Instance, _clock);

    private void SetupEmptyFeed() =>
        _repo
            .Setup(x => x.GetFeedAsync(It.IsAny<JobFeedFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListDataPagination<JobFeedProjection>([], 0, 1, 20));

    private JobFeedFilter CapturedFilter()
    {
        JobFeedFilter? captured = null;
        _repo.Verify(x => x.GetFeedAsync(
            It.Is<JobFeedFilter>(f => Capture(f, ref captured)),
            It.IsAny<CancellationToken>()), Times.Once);

        captured.Should().NotBeNull();
        return captured!;
    }

    private static bool Capture(JobFeedFilter filter, ref JobFeedFilter? slot)
    {
        slot = filter;
        return true;
    }

    [Fact]
    public async Task Handle_SemFiltroDeData_NaoDeveRestringirPorPublicacao()
    {
        SetupEmptyFeed();

        await CreateSut().Handle(new GetJobsFeedQuery(), CancellationToken.None);

        CapturedFilter().PublishedAfter.Should().BeNull();
    }

    [Fact]
    public async Task Handle_JanelaHoje_DeveUsarMeiaNoiteDeBrasiliaENaoDeUtc()
    {
        SetupEmptyFeed();

        // 02:30 UTC de 02/08 é 23:30 de 01/08 em Brasília: "hoje" para o utilizador ainda é dia 1.
        // Usar o dia UTC aqui esvaziaria o filtro todas as noites.
        await CreateSut().Handle(
            new GetJobsFeedQuery(PublishedWithin: JobPublishedWindowEnum.Today),
            CancellationToken.None);

        var publishedAfter = CapturedFilter().PublishedAfter;
        publishedAfter.Should().NotBeNull();

        var inBrasilia = TimeZoneInfo.ConvertTime(publishedAfter!.Value, BrasiliaTime.GetBrasiliaTimeZone());
        inBrasilia.Day.Should().Be(1);
        inBrasilia.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task Handle_JanelaUltimas24Horas_DeveDiferirDeHoje()
    {
        SetupEmptyFeed();
        var sut = CreateSut();

        await sut.Handle(new GetJobsFeedQuery(PublishedWithin: JobPublishedWindowEnum.Today), CancellationToken.None);
        var today = CapturedFilter().PublishedAfter;

        _repo.Invocations.Clear();
        await sut.Handle(new GetJobsFeedQuery(PublishedWithin: JobPublishedWindowEnum.Last24Hours), CancellationToken.None);
        var last24h = CapturedFilter().PublishedAfter;

        last24h.Should().Be(_clock.GetUtcNow().AddHours(-24));
        last24h.Should().NotBe(today);
    }

    [Theory]
    [InlineData(JobPublishedWindowEnum.Last3Days, 3)]
    [InlineData(JobPublishedWindowEnum.Last7Days, 7)]
    [InlineData(JobPublishedWindowEnum.Last15Days, 15)]
    [InlineData(JobPublishedWindowEnum.Last30Days, 30)]
    public async Task Handle_JanelaEmDias_DeveContarParaTrasAPartirDeAgora(
        JobPublishedWindowEnum window, int expectedDays)
    {
        SetupEmptyFeed();

        await CreateSut().Handle(new GetJobsFeedQuery(PublishedWithin: window), CancellationToken.None);

        CapturedFilter().PublishedAfter.Should().Be(_clock.GetUtcNow().AddDays(-expectedDays));
    }

    [Fact]
    public async Task Handle_BuscaSoComEspacos_DeveTratarComoAusenteEmVezDeBuscarVazio()
    {
        SetupEmptyFeed();

        await CreateSut().Handle(new GetJobsFeedQuery(Search: "   "), CancellationToken.None);

        var filter = CapturedFilter();
        filter.Search.Should().BeNull();
        filter.HasSearch.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_BuscaComEspacosNasBordas_DeveEnviarTermoAparado()
    {
        SetupEmptyFeed();

        await CreateSut().Handle(new GetJobsFeedQuery(Search: "  react  "), CancellationToken.None);

        CapturedFilter().Search.Should().Be("react");
    }

    [Fact]
    public async Task Handle_FiltrosCombinados_DevePropagarTodosOsGrupos()
    {
        SetupEmptyFeed();

        var query = new GetJobsFeedQuery(
            Search: "backend",
            Cities: ["Extrema"],
            States: [UF.MG, UF.SP],
            WorkModels: [WorkModelEnum.Remote, WorkModelEnum.Hybrid],
            JobTypes: [JobTypeEnum.Clt],
            WorkShifts: [WorkShiftEnum.SegundoTurno],
            ExperienceLevels: [ExperienceLevelEnum.SemExperiencia],
            Areas: [JobAreaEnum.Logistica],
            Requirements: ["Empilhadeira", "WMS"],
            Benefits: ["Fretado"],
            CompanyIds: [7L],
            SalaryMin: 4000m,
            SalaryMax: 9000m);

        await CreateSut().Handle(query, CancellationToken.None);

        var filter = CapturedFilter();
        filter.Cities.Should().BeEquivalentTo(["Extrema"]);
        filter.States.Should().BeEquivalentTo([UF.MG, UF.SP]);
        filter.WorkModels.Should().BeEquivalentTo([WorkModelEnum.Remote, WorkModelEnum.Hybrid]);
        filter.JobTypes.Should().BeEquivalentTo([JobTypeEnum.Clt]);
        filter.WorkShifts.Should().BeEquivalentTo([WorkShiftEnum.SegundoTurno]);
        filter.ExperienceLevels.Should().BeEquivalentTo([ExperienceLevelEnum.SemExperiencia]);
        filter.Areas.Should().BeEquivalentTo([JobAreaEnum.Logistica]);
        filter.Requirements.Should().BeEquivalentTo(["Empilhadeira", "WMS"]);
        filter.Benefits.Should().BeEquivalentTo(["Fretado"]);
        filter.CompanyIds.Should().BeEquivalentTo([7L]);
        filter.SalaryMin.Should().Be(4000m);
        filter.SalaryMax.Should().Be(9000m);
        filter.HasSalaryBound.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SemLimiteSalarial_NaoDeveExcluirVagasComSalarioACombinar()
    {
        SetupEmptyFeed();

        await CreateSut().Handle(new GetJobsFeedQuery(), CancellationToken.None);

        CapturedFilter().HasSalaryBound.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DeveMapearProjecaoParaOCartaoIncluindoContagemDeCandidatos()
    {
        var projection = new JobFeedProjection(
            Id: 42,
            Title: "Operador(a) de Empilhadeira",
            Summary: "Centro de distribuição, 2º turno",
            Company: new JobFeedCompany(3, "Acme Logística"),
            Location: new JobFeedLocation("Extrema", UF.MG, "BR"),
            Salary: new JobFeedSalary(2300m, 2800m, true),
            JobType: JobTypeEnum.Clt,
            WorkModel: WorkModelEnum.OnSite,
            WorkShift: WorkShiftEnum.SegundoTurno,
            ExperienceLevel: ExperienceLevelEnum.AteUmAno,
            Area: JobAreaEnum.Logistica,
            IsPcdFriendly: false,
            Requirements: ["Empilhadeira"],
            Benefits: ["Fretado"],
            PublishedAt: new DateTimeOffset(2026, 7, 28, 13, 45, 0, TimeSpan.Zero),
            ApplicationsCount: 12,
            IsActive: true);

        _repo
            .Setup(x => x.GetFeedAsync(It.IsAny<JobFeedFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListDataPagination<JobFeedProjection>([projection], 1, 1, 20));

        var result = await CreateSut().Handle(new GetJobsFeedQuery(), CancellationToken.None);

        var item = result.Data.Should().ContainSingle().Subject;
        item.Id.Should().Be(42);
        item.Company.Name.Should().Be("Acme Logística");
        item.Location.State.Should().Be("MG");
        item.Salary.Min.Should().Be(2300m);
        item.Salary.Disclosed.Should().BeTrue();
        item.WorkShift.Should().Be("SegundoTurno");
        item.Requirements.Should().BeEquivalentTo(["Empilhadeira"]);
        item.ApplicationsCount.Should().Be(12);
        result.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Handle_DeveExporEnumsComoNomeENaoComoInteiro()
    {
        var projection = new JobFeedProjection(
            1, "Vaga", null,
            new JobFeedCompany(1, "Acme"),
            new JobFeedLocation("Extrema", UF.MG, "BR"),
            new JobFeedSalary(null, null, false),
            JobTypeEnum.Pj, WorkModelEnum.Remote, WorkShiftEnum.TerceiroTurno,
            ExperienceLevelEnum.MaisDeCincoAnos, JobAreaEnum.Manutencao, true,
            [], [], DateTimeOffset.UtcNow, 0, true);

        _repo
            .Setup(x => x.GetFeedAsync(It.IsAny<JobFeedFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListDataPagination<JobFeedProjection>([projection], 1, 1, 20));

        var result = await CreateSut().Handle(new GetJobsFeedQuery(), CancellationToken.None);

        var item = result.Data.Single();
        item.JobType.Should().Be("Pj");
        item.WorkModel.Should().Be("Remote");
        item.WorkShift.Should().Be("TerceiroTurno");
        item.ExperienceLevel.Should().Be("MaisDeCincoAnos");
        item.Area.Should().Be("Manutencao");
        item.IsPcdFriendly.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DevePropagarPaginacaoSolicitada()
    {
        SetupEmptyFeed();

        await CreateSut().Handle(new GetJobsFeedQuery(Page: 3, Size: 15), CancellationToken.None);

        var filter = CapturedFilter();
        filter.Page.Should().Be(3);
        filter.Size.Should().Be(15);
    }
}

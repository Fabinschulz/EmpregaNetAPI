using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Jobs.Queries;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.Jobs;

/// <summary>
/// A contagem de candidaturas em aberto é informação da empresa e tem de ser fresca.
/// </summary>
/// <remarks>
/// Estava a ser servida como campo de <c>JobViewModel</c>, ou seja por <c>GET /api/jobs/{id}</c>, que
/// é <c>[AllowAnonymous]</c> e cacheado como catálogo público. Isso expunha a concorrência de cada
/// vaga a qualquer visitante e entregava ao recrutador um número com minutos de atraso — uma
/// confirmação de encerramento capaz de afirmar "nenhuma candidatura será cancelada" sobre uma vaga
/// com fila. Este ficheiro fixa a correcção: endpoint próprio, autenticado, sem cache.
/// </remarks>
public sealed class GetJobOpenApplicationsCountHandlerTests
{
    private const long JobId = 4242;
    private const long CompanyId = 77;

    private readonly Mock<IJobRepository> _jobs = new();
    private readonly Mock<IJobApplicationRepository> _applications = new();
    private readonly Mock<IJobEmployerAccess> _employerAccess = new();
    private readonly Mock<IHttpCurrentUser> _currentUser = new();

    private GetJobOpenApplicationsCountHandler CreateSut() =>
        new(_jobs.Object,
            _applications.Object,
            _employerAccess.Object,
            _currentUser.Object,
            NullLogger<GetJobOpenApplicationsCountHandler>.Instance);

    private static Job CreateJob(bool isDeleted = false)
    {
        var job = new Job(
            companyId: CompanyId,
            title: "Operador de Empilhadeira",
            description: "Movimentacao de cargas.",
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.SegundoTurno,
            experienceLevel: ExperienceLevelEnum.AteUmAno,
            area: JobAreaEnum.Logistica,
            location: new JobLocation { City = "Extrema", State = UF.MG },
            salaryMin: 2300m);

        job.IsDeleted = isDeleted;
        return job;
    }

    private void GivenRecruiter(string role = "Recruiter")
    {
        _currentUser.SetupGet(x => x.UserId).Returns(900);
        _currentUser.Setup(x => x.GetContextUser()).Returns(new UserLoggedViewModel
        {
            AccessToken = "token",
            ExpiresIn = 3600,
            UserToken = new UserToken
            {
                Id = 900,
                Username = "recrutador",
                Email = "recrutador@test.local",
                Roles = [role],
                Claims = []
            }
        });
    }

    private void GivenJob(Job? job) =>
        _jobs.Setup(x => x.GetByIdAsync(JobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

    private void GivenAccessAllowed() =>
        _employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    private void GivenCount(int count) =>
        _applications
            .Setup(x => x.CountOpenByJobIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

    [Fact]
    public async Task Handle_VagaComCandidaturasEmAberto_DeveDevolverAContagem()
    {
        GivenRecruiter();
        var job = CreateJob();
        GivenJob(job);
        GivenAccessAllowed();
        GivenCount(3);

        var result = await CreateSut().Handle(new GetJobOpenApplicationsCountQuery(JobId), CancellationToken.None);

        // A entidade do teste não passou pelo banco, logo Id fica 0. O que interessa afirmar é que a
        // resposta identifica a vaga resolvida e que a contagem é contada por ela.
        result.JobId.Should().Be(job.Id);
        result.OpenApplicationsCount.Should().Be(3);
        _applications.Verify(x => x.CountOpenByJobIdAsync(job.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_VagaSemCandidaturasEmAberto_DeveDevolverZero()
    {
        GivenRecruiter();
        GivenJob(CreateJob());
        GivenAccessAllowed();
        GivenCount(0);

        var result = await CreateSut().Handle(new GetJobOpenApplicationsCountQuery(JobId), CancellationToken.None);

        result.OpenApplicationsCount.Should().Be(0);
    }

    /// <summary>
    /// Pertencer ao recrutamento não basta: a fila de candidatos de uma vaga é informação competitiva
    /// da empresa que a publicou.
    /// </summary>
    [Fact]
    public async Task Handle_RecrutadorDeOutraEmpresa_DeveRecusarSemContar()
    {
        GivenRecruiter();
        GivenJob(CreateJob());
        _employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(ValidationAppException.ForBusinessRule(
                "Sem acesso à empresa desta vaga.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION));

        var act = async () => await CreateSut().Handle(new GetJobOpenApplicationsCountQuery(JobId), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
        _applications.Verify(
            x => x.CountOpenByJobIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DeveVerificarOAcessoDaEmpresaDonaDaVaga()
    {
        GivenRecruiter();
        GivenJob(CreateJob());
        GivenAccessAllowed();
        GivenCount(1);

        await CreateSut().Handle(new GetJobOpenApplicationsCountQuery(JobId), CancellationToken.None);

        _employerAccess.Verify(
            x => x.EnsureCanManageCompanyAsync(CompanyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // Defesa em profundidade além do [Authorize] do endpoint, como CloseJobHandler já faz.
    [Fact]
    public async Task Handle_PerfilSemRecrutamento_DeveRecusarAntesDeConsultarAVaga()
    {
        GivenRecruiter(role: "Candidate");

        var act = async () => await CreateSut().Handle(new GetJobOpenApplicationsCountQuery(JobId), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        _jobs.Verify(x => x.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_VagaInexistente_DeveLancarNotFound()
    {
        GivenRecruiter();
        GivenJob(null);

        var act = async () => await CreateSut().Handle(new GetJobOpenApplicationsCountQuery(JobId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_VagaExcluida_DeveLancarNotFound()
    {
        GivenRecruiter();
        GivenJob(CreateJob(isDeleted: true));

        var act = async () => await CreateSut().Handle(new GetJobOpenApplicationsCountQuery(JobId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    /// <summary>
    /// Regressão do defeito que motivou o endpoint: o detalhe público da vaga <b>não</b> pode carregar
    /// a contagem. Se alguém voltar a acrescentar a propriedade ao <c>JobViewModel</c>, ela volta a
    /// sair por <c>GET /api/jobs/{id}</c> — anónimo e cacheado — e este teste falha.
    /// </summary>
    [Fact]
    public void JobViewModel_NaoDeveExporContagemDeCandidaturas()
    {
        var propriedades = typeof(JobViewModel).GetProperties().Select(p => p.Name);

        propriedades.Should().NotContain("OpenApplicationsCount");
    }
}

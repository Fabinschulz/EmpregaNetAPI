using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.Jobs;

/// <summary>
/// Regressão do bug de produção (ref. correlationId 2b0e1386-50d8-45a8-97b9-57dbd6f3608e): excluir uma
/// vaga devolvia 500 "Erro interno no servidor" em vez da mensagem de negócio, porque o handler tinha
/// um <c>catch (Exception ex)</c> genérico que engolia <b>qualquer</b> falha - inclusive
/// <see cref="ValidationAppException"/> legítima - e relançava um <see cref="Exception"/> cru, que o
/// <c>GlobalExceptionHandler</c> não sabe mapear e por isso sempre vira 500.
/// </summary>
public sealed class DeleteJobHandlerTests
{
    private const long JobId = 55;
    private const long CompanyId = 77;

    private readonly Mock<IJobRepository> _jobs = new();
    private readonly Mock<IJobApplicationRepository> _jobApplications = new();
    private readonly Mock<IHttpCurrentUser> _currentUser = new();
    private readonly Mock<IJobEmployerAccess> _employerAccess = new();

    private DeleteJobHandler CreateSut() =>
        new(_jobs.Object,
            _jobApplications.Object,
            NullLogger<DeleteJobHandler>.Instance,
            _currentUser.Object,
            _employerAccess.Object);

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

    private void GivenApplicationsForJob(bool exists) =>
        _jobApplications
            .Setup(x => x.ExistsByJobIdAsync(JobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);

    [Fact]
    public async Task Handle_VagaExistente_DeveExcluirComSucesso()
    {
        GivenRecruiter();
        GivenJob(CreateJob());
        GivenApplicationsForJob(exists: false);
        _employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(CompanyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _jobs.Setup(x => x.DeleteAsync(JobId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().Handle(new DeleteCommand<JobViewModel>(JobId), CancellationToken.None);

        result.Should().BeTrue();
        _jobs.Verify(x => x.DeleteAsync(JobId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_VagaInexistente_DeveLancarValidationAppExceptionComCodigoCorreto()
    {
        GivenRecruiter();
        GivenJob(null);

        var act = async () => await CreateSut().Handle(new DeleteCommand<JobViewModel>(JobId), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
    }

    /// <summary>
    /// Regressão directa do bug: uma <see cref="ValidationAppException"/> de negócio (aqui, acesso
    /// negado à empresa da vaga) tem de chegar ao <c>GlobalExceptionHandler</c> com o seu tipo e
    /// <see cref="ValidationAppException.Code"/> intactos - nunca embrulhada num
    /// <see cref="Exception"/> genérico, que só produz 500.
    /// </summary>
    [Fact]
    public async Task Handle_FalhaDeNegocioAoVerificarAcesso_DevePropagarValidationAppExceptionSemEmbrulhar()
    {
        GivenRecruiter();
        GivenJob(CreateJob());
        _employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(CompanyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(ValidationAppException.ForBusinessRule(
                "Você só pode gerenciar vagas da empresa à qual está vinculado.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION));

        var act = async () => await CreateSut().Handle(new DeleteCommand<JobViewModel>(JobId), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        _jobs.Verify(x => x.DeleteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Sem FK entre <c>Job</c> e <c>JobApplication</c>, excluir a vaga deixaria candidaturas reais
    /// (histórico de candidatos) apontando para um registro que sumiu. A regra bloqueia a exclusão
    /// enquanto existir qualquer candidatura vinculada, independentemente do status.
    /// </summary>
    [Fact]
    public async Task Handle_VagaComCandidaturaVinculada_DeveLancarValidationAppExceptionEBloquearExclusao()
    {
        GivenRecruiter();
        GivenJob(CreateJob());
        GivenApplicationsForJob(exists: true);
        _employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(CompanyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var act = async () => await CreateSut().Handle(new DeleteCommand<JobViewModel>(JobId), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
        _jobs.Verify(x => x.DeleteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_VagaSemCandidatura_DeveExcluirNormalmente()
    {
        GivenRecruiter();
        GivenJob(CreateJob());
        GivenApplicationsForJob(exists: false);
        _employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(CompanyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _jobs.Setup(x => x.DeleteAsync(JobId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().Handle(new DeleteCommand<JobViewModel>(JobId), CancellationToken.None);

        result.Should().BeTrue();
        _jobs.Verify(x => x.DeleteAsync(JobId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

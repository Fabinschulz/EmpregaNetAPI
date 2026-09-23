using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Commands;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.JobApplications;

/// <summary>
/// Excluir uma candidatura é permitido em qualquer status de processo em aberto ou de desfecho
/// negativo/cancelado. É bloqueado apenas em <see cref="ApplicationStatusEnum.Finished"/> ("Concluída"),
/// que representa uma contratação já efetivada — histórico que não deve ser apagável.
/// </summary>
public sealed class DeleteJobApplicationHandlerTests
{
    private const long ApplicationId = 300;
    private const long JobId = 55;
    private const long CompanyId = 9;

    private readonly Mock<IJobApplicationRepository> _jobApplications = new();
    private readonly Mock<IJobRepository> _jobs = new();
    private readonly Mock<IJobEmployerAccess> _employerAccess = new();

    private DeleteJobApplicationHandler CreateSut() =>
        new(_jobApplications.Object,
            _jobs.Object,
            _employerAccess.Object,
            NullLogger<DeleteJobApplicationHandler>.Instance);

    private static Job CreateJob(bool isDeleted = false)
    {
        var job = new Job(
            companyId: CompanyId,
            title: "Auxiliar de Produção",
            description: "Linha de montagem.",
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.PrimeiroTurno,
            experienceLevel: ExperienceLevelEnum.AteUmAno,
            area: JobAreaEnum.Logistica,
            location: new JobLocation { City = "Extrema", State = UF.MG },
            salaryMin: 2100m);

        job.IsDeleted = isDeleted;
        return job;
    }

    private static JobApplication CreateApplication(ApplicationStatusEnum status)
    {
        var application = new JobApplication(JobId, userId: 1);

        if (status == ApplicationStatusEnum.CanceledByCandidate)
        {
            application.CancelByCandidate();
        }
        else if (status != ApplicationStatusEnum.Pending)
        {
            application.ChangeStatus(status);
        }

        return application;
    }

    private void GivenApplication(JobApplication? application) =>
        _jobApplications
            .Setup(x => x.GetByIdAsync(ApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

    private void GivenJob(Job? job) =>
        _jobs.Setup(x => x.GetByIdAsync(JobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

    private void GivenEmployerAccessGranted() =>
        _employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(CompanyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    [Theory]
    [InlineData(ApplicationStatusEnum.Pending)]
    [InlineData(ApplicationStatusEnum.Processing)]
    [InlineData(ApplicationStatusEnum.Approved)]
    [InlineData(ApplicationStatusEnum.Rejected)]
    [InlineData(ApplicationStatusEnum.Canceled)]
    [InlineData(ApplicationStatusEnum.CanceledByCandidate)]
    public async Task Handle_StatusNaoConcluido_DeveExcluirComSucesso(ApplicationStatusEnum status)
    {
        GivenApplication(CreateApplication(status));
        GivenJob(CreateJob());
        GivenEmployerAccessGranted();
        _jobApplications.Setup(x => x.DeleteAsync(ApplicationId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().Handle(new DeleteCommand<JobApplicationViewModel>(ApplicationId), CancellationToken.None);

        result.Should().BeTrue();
        _jobApplications.Verify(x => x.DeleteAsync(ApplicationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CandidaturaConcluida_DeveLancarValidationAppExceptionEBloquearExclusao()
    {
        GivenApplication(CreateApplication(ApplicationStatusEnum.Finished));
        GivenJob(CreateJob());
        GivenEmployerAccessGranted();

        var act = async () => await CreateSut().Handle(new DeleteCommand<JobApplicationViewModel>(ApplicationId), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
        _jobApplications.Verify(x => x.DeleteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CandidaturaInexistente_DeveLancarValidationAppExceptionComCodigoCorreto()
    {
        GivenApplication(null);

        var act = async () => await CreateSut().Handle(new DeleteCommand<JobApplicationViewModel>(ApplicationId), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_FalhaDeNegocioAoVerificarAcesso_DevePropagarValidationAppExceptionSemEmbrulhar()
    {
        GivenApplication(CreateApplication(ApplicationStatusEnum.Pending));
        GivenJob(CreateJob());
        _employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(CompanyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(ValidationAppException.ForBusinessRule(
                "Você só pode gerenciar vagas da empresa à qual está vinculado.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION));

        var act = async () => await CreateSut().Handle(new DeleteCommand<JobApplicationViewModel>(ApplicationId), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        _jobApplications.Verify(x => x.DeleteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_VagaAssociadaInexistente_DeveLancarValidationAppExceptionComCodigoCorreto()
    {
        GivenApplication(CreateApplication(ApplicationStatusEnum.Pending));
        GivenJob(null);

        var act = async () => await CreateSut().Handle(new DeleteCommand<JobApplicationViewModel>(ApplicationId), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        _jobApplications.Verify(x => x.DeleteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Commands;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.JobApplications;

/// <summary>
/// Como a recusa do agregado chega ao recrutador. As guardas de <c>ChangeStatus</c> lançam
/// <see cref="InvalidOperationException"/>, que o tratamento global mapeia para <c>409</c> com a
/// frase genérica "Operação inválida." — código que este endpoint nem declara. O handler traduz para
/// <c>400</c> com <c>INVALID_ACTION_FOR_STATUS</c>, que é o que o cliente sabe apresentar.
/// </summary>
public sealed class ChangeJobApplicationStatusHandlerTests
{
    private const long ApplicationId = 300;
    private const long JobId = 55;
    private const long CompanyId = 9;

    private readonly Mock<IJobApplicationRepository> _applications = new();
    private readonly Mock<IJobRepository> _jobs = new();
    private readonly Mock<IJobEmployerAccess> _employerAccess = new();

    private ChangeJobApplicationStatusCommandHandler CreateSut() =>
        new(_applications.Object,
            _jobs.Object,
            _employerAccess.Object,
            NullLogger<ChangeJobApplicationStatusCommandHandler>.Instance);

    private static Job CreateJob() => new(
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

    private void GivenApplication(JobApplication application)
    {
        _applications
            .Setup(x => x.GetByIdAsync(ApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);
        _applications
            .Setup(x => x.UpdateAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);
        _applications
            .Setup(x => x.GetProjectionByIdAsync(ApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new JobApplicationProjection(
                ApplicationId,
                JobId,
                new JobApplicationCandidate(1, "Candidato", "candidato@test.local", false),
                application.Status,
                application.AppliedAt,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null,
                false));

        _jobs.Setup(x => x.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(CreateJob());
        _employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private Task<JobApplicationViewModel> ExecuteAsync(string status) =>
        CreateSut().Handle(
            new UpdateCommand<ChangeJobApplicationStatusCommand, JobApplicationViewModel>(
                ApplicationId,
                new ChangeJobApplicationStatusCommand(status)),
            CancellationToken.None);

    [Fact]
    public async Task Handle_TransicaoValida_DeveActualizarOStatus()
    {
        var application = new JobApplication(JobId, userId: 1);
        GivenApplication(application);

        var result = await ExecuteAsync(nameof(ApplicationStatusEnum.Processing));

        result.Status.Should().Be(ApplicationStatusEnum.Processing);
        application.Status.Should().Be(ApplicationStatusEnum.Processing);
    }

    // RBAC-2: o recrutador não cancela em nome do candidato.
    [Fact]
    public async Task Handle_DestinoCanceladoPeloCandidato_DeveDevolver400ENaoConflito()
    {
        var application = new JobApplication(JobId, userId: 1);
        GivenApplication(application);

        var act = async () => await ExecuteAsync(nameof(ApplicationStatusEnum.CanceledByCandidate));

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        _applications.Verify(x => x.UpdateAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // X3/X6: o ato do candidato é terminal e a empresa não o sobrepõe.
    [Fact]
    public async Task Handle_CandidaturaCanceladaPeloCandidato_DeveDevolver400ENaoConflito()
    {
        var application = new JobApplication(JobId, userId: 1);
        application.CancelByCandidate();
        GivenApplication(application);

        var act = async () => await ExecuteAsync(nameof(ApplicationStatusEnum.Approved));

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        application.Status.Should().Be(ApplicationStatusEnum.CanceledByCandidate);
    }

    [Fact]
    public async Task Handle_StatusIgualAoActual_DeveDevolver400ENaoConflito()
    {
        var application = new JobApplication(JobId, userId: 1);
        GivenApplication(application);

        var act = async () => await ExecuteAsync(nameof(ApplicationStatusEnum.Pending));

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
    }
}

using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Commands;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Application.JobApplications.Events;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Tests.Support;
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
    private readonly Mock<IJobClosureCascade> _closureCascade = new();
    private readonly RecordingDomainEventQueue _domainEvents = new();

    private ChangeJobApplicationStatusCommandHandler CreateSut() =>
        new(_applications.Object,
            _jobs.Object,
            _employerAccess.Object,
            _closureCascade.Object,
            _domainEvents,
            NullLogger<ChangeJobApplicationStatusCommandHandler>.Instance);

    private static Job CreateJob(int positions = 5) => new(
        companyId: CompanyId,
        title: "Auxiliar de Produção",
        description: "Linha de montagem.",
        jobType: JobTypeEnum.Clt,
        workModel: WorkModelEnum.OnSite,
        workShift: WorkShiftEnum.PrimeiroTurno,
        experienceLevel: ExperienceLevelEnum.AteUmAno,
        area: JobAreaEnum.Logistica,
        location: new JobLocation { City = "Extrema", State = UF.MG },
        positions: positions,
        salaryMin: 2100m);

    private void GivenApplication(JobApplication application) => GivenApplication(application, CreateJob());

    private void GivenApplication(JobApplication application, Job job)
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
                job.Title,
                new JobApplicationCandidate(1, "Candidato", "candidato@test.local", false),
                application.Status,
                application.AppliedAt,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null,
                false));

        // O handler lê a vaga com a linha bloqueada; é este o método que ele chama.
        _jobs.Setup(x => x.GetByIdForUpdateAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(job);
        _jobs.Setup(x => x.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(job);
        _employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _closureCascade
            .Setup(x => x.CancelOpenApplicationsAsync(
                It.IsAny<long>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<JobApplicationNotificationReason>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
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

    // N2-N5: cada transição válida do recrutador tem de deixar exactamente um evento na fila, com a
    // razão que escolhe o texto do e-mail.
    [Theory]
    [InlineData(ApplicationStatusEnum.Processing)]
    [InlineData(ApplicationStatusEnum.Approved)]
    [InlineData(ApplicationStatusEnum.Rejected)]
    [InlineData(ApplicationStatusEnum.Finished)]
    public async Task Handle_TransicaoValida_DeveEnfileirarUmEventoDeMudancaDeStatus(ApplicationStatusEnum destino)
    {
        var application = new JobApplication(JobId, userId: 1);
        GivenApplication(application);

        await ExecuteAsync(destino.ToString());

        var evento = _domainEvents.RecordedOf<JobApplicationStatusChanged>().Should().ContainSingle().Subject;
        // A entidade do teste não passou pelo banco, logo Id fica 0; o que interessa aqui é a vaga,
        // o candidato, os dois estados e a razão — é isso que decide o e-mail.
        evento.JobId.Should().Be(JobId);
        evento.CandidateUserId.Should().Be(1);
        evento.PreviousStatus.Should().Be(ApplicationStatusEnum.Pending);
        evento.NewStatus.Should().Be(destino);
        evento.Reason.Should().Be(JobApplicationNotificationReason.StatusChanged);
    }

    // CA-04: repetir a mesma transição não pode gerar um segundo e-mail. A recusa acontece antes do
    // enfileiramento, portanto não há evento para publicar.
    [Fact]
    public async Task Handle_TransicaoRecusada_NaoDeveEnfileirarEvento()
    {
        var application = new JobApplication(JobId, userId: 1);
        GivenApplication(application);

        var act = async () => await ExecuteAsync(nameof(ApplicationStatusEnum.Pending));
        await act.Should().ThrowAsync<ValidationAppException>();

        _domainEvents.Recorded.Should().BeEmpty();
    }

    // ----------------------------------------------------------------------------------------
    // Posições da vaga
    // ----------------------------------------------------------------------------------------

    [Fact]
    public async Task Handle_Aprovacao_DeveOcuparUmaPosicaoDaVaga()
    {
        var job = CreateJob(positions: 3);
        var application = new JobApplication(JobId, userId: 1);
        GivenApplication(application, job);

        await ExecuteAsync(nameof(ApplicationStatusEnum.Approved));

        job.FilledPositions.Should().Be(1);
        job.AvailablePositions.Should().Be(2);
        job.IsActive.Should().BeTrue("ainda sobram posições");
        _jobs.Verify(x => x.UpdateAsync(job, It.IsAny<CancellationToken>()), Times.Once);
    }

    // O coração da regra: a última aprovação encerra a vaga e arrasta quem ficou pelo caminho.
    [Fact]
    public async Task Handle_UltimaAprovacao_DeveEncerrarAVagaECancelarAsCandidaturasEmAberto()
    {
        var job = CreateJob(positions: 1);
        var application = new JobApplication(JobId, userId: 1);
        GivenApplication(application, job);

        await ExecuteAsync(nameof(ApplicationStatusEnum.Approved));

        job.AvailablePositions.Should().Be(0);
        job.IsActive.Should().BeFalse();
        job.ClosureReason.Should().Be(JobClosureReasonEnum.Fulfilled);

        _closureCascade.Verify(
            x => x.CancelOpenApplicationsAsync(
                job.Id,
                job.ClosedAt!.Value,
                JobApplicationNotificationReason.JobFilled,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_AprovacaoComPosicoesSobrando_NaoDeveArrastarCandidaturas()
    {
        var job = CreateJob(positions: 2);
        GivenApplication(new JobApplication(JobId, userId: 1), job);

        await ExecuteAsync(nameof(ApplicationStatusEnum.Approved));

        _closureCascade.Verify(
            x => x.CancelOpenApplicationsAsync(
                It.IsAny<long>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<JobApplicationNotificationReason>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Concluir o processo de quem foi aprovado não devolve a posição, senão a vaga reabria sozinha.
    [Fact]
    public async Task Handle_AprovadoParaConcluido_NaoDeveLibertarAPosicao()
    {
        var job = CreateJob(positions: 2);
        var application = new JobApplication(JobId, userId: 1);
        application.ChangeStatus(ApplicationStatusEnum.Approved);
        job.FillPosition();
        GivenApplication(application, job);

        await ExecuteAsync(nameof(ApplicationStatusEnum.Finished));

        job.FilledPositions.Should().Be(1);
        _jobs.Verify(x => x.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AprovadoParaReprovado_DeveLibertarAPosicao()
    {
        var job = CreateJob(positions: 2);
        var application = new JobApplication(JobId, userId: 1);
        application.ChangeStatus(ApplicationStatusEnum.Approved);
        job.FillPosition();
        GivenApplication(application, job);

        await ExecuteAsync(nameof(ApplicationStatusEnum.Rejected));

        job.FilledPositions.Should().Be(0);
        job.AvailablePositions.Should().Be(2);
        _jobs.Verify(x => x.UpdateAsync(job, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Uma vaga encerrada não tem posição para dar. A recusa do agregado tem de chegar como 400,
    // não como 500 nem como conflito genérico.
    [Fact]
    public async Task Handle_AprovacaoEmVagaEncerrada_DeveDevolver400ENaoConflito()
    {
        var job = CreateJob(positions: 1);
        job.Close();
        GivenApplication(new JobApplication(JobId, userId: 1), job);

        var act = async () => await ExecuteAsync(nameof(ApplicationStatusEnum.Approved));

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        _applications.Verify(x => x.UpdateAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()), Times.Never);
        _domainEvents.Recorded.Should().BeEmpty();
    }

    // Transição que não mexe em posições não escreve na vaga: menos uma linha tocada por engano.
    [Fact]
    public async Task Handle_TransicaoSemEfeitoEmPosicoes_NaoDeveEscreverNaVaga()
    {
        var job = CreateJob(positions: 3);
        GivenApplication(new JobApplication(JobId, userId: 1), job);

        await ExecuteAsync(nameof(ApplicationStatusEnum.Processing));

        job.FilledPositions.Should().Be(0);
        _jobs.Verify(x => x.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

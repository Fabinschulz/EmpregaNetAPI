using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Application.JobApplications.Events;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.JobApplications;

/// <summary>
/// Duas garantias, e a segunda é a que protege o negócio:
/// <list type="number">
///   <item>O destinatário e o conteúdo vêm do <b>registo</b>, nunca de dado da requisição (CA-08).</item>
///   <item>O handler <b>nunca relança</b>: corre depois do commit, e uma falha de e-mail não pode
///         desfazer nem sinalizar erro numa transição já gravada (CA-03, W1/W2).</item>
/// </list>
/// </summary>
public sealed class JobApplicationStatusChangedEmailHandlerTests
{
    private const long ApplicationId = 900;
    private const long CandidateId = 77;

    private readonly Mock<IJobApplicationRepository> _repository = new();
    private readonly Mock<IJobApplicationEmailService> _emailService = new();

    private JobApplicationStatusChangedEmailHandler CreateSut() =>
        new(_repository.Object,
            _emailService.Object,
            Options.Create(new AppUrlsOptions
            {
                PublicAppBaseUrl = "https://app.test/",
                ApplicationsPath = "/candidaturas"
            }),
            NullLogger<JobApplicationStatusChangedEmailHandler>.Instance);

    private static JobApplicationStatusChanged CreateEvent(
        ApplicationStatusEnum newStatus = ApplicationStatusEnum.Approved,
        JobApplicationNotificationReason reason = JobApplicationNotificationReason.StatusChanged) => new(
        JobApplicationId: ApplicationId,
        JobId: 12,
        CandidateUserId: CandidateId,
        PreviousStatus: ApplicationStatusEnum.Processing,
        NewStatus: newStatus,
        OccurredAt: new DateTimeOffset(2026, 9, 2, 12, 0, 0, TimeSpan.Zero),
        Reason: reason);

    private void GivenProjection(JobApplicationNotificationProjection? projection) =>
        _repository
            .Setup(x => x.GetNotificationProjectionAsync(ApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projection);

    private static JobApplicationNotificationProjection CreateProjection(string email = "candidato@test.local") => new(
        JobApplicationId: ApplicationId,
        JobId: 12,
        JobTitle: "Operador de Empilhadeira",
        CompanyName: "Metalurgica Extrema",
        CandidateId: CandidateId,
        CandidateName: "Ana",
        CandidateEmail: email);


    [Fact]
    public async Task Handle_EventoDeAprovacao_DeveEnviarComOsDadosDoRegisto()
    {
        GivenProjection(CreateProjection());
        JobApplicationEmailModel? sent = null;
        _emailService
            .Setup(x => x.SendStatusNotificationAsync(It.IsAny<JobApplicationEmailModel>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplicationEmailModel, CancellationToken>((m, _) => sent = m)
            .Returns(Task.CompletedTask);

        await CreateSut().Handle(CreateEvent(), CancellationToken.None);

        sent.Should().NotBeNull();
        sent!.CandidateEmail.Should().Be("candidato@test.local");
        sent.JobTitle.Should().Be("Operador de Empilhadeira");
        sent.CompanyName.Should().Be("Metalurgica Extrema");
        sent.StatusDescription.Should().Be("Aprovado", "é a descrição pt-BR do enum, como o candidato a vê");
        sent.ApplicationsUrl.Should().Be("https://app.test/candidaturas");
        sent.Reason.Should().Be(JobApplicationNotificationReason.StatusChanged);
    }

    // O destinatário é resolvido pela candidatura, nunca por um id que tenha chegado no pedido.
    [Fact]
    public async Task Handle_DeveResolverODestinatarioPelaCandidaturaDoEvento()
    {
        GivenProjection(CreateProjection());
        _emailService
            .Setup(x => x.SendStatusNotificationAsync(It.IsAny<JobApplicationEmailModel>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateSut().Handle(CreateEvent(), CancellationToken.None);

        _repository.Verify(
            x => x.GetNotificationProjectionAsync(ApplicationId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(ApplicationStatusEnum.Processing, "Em Análise")]
    [InlineData(ApplicationStatusEnum.Approved, "Aprovado")]
    [InlineData(ApplicationStatusEnum.Rejected, "Rejeitado")]
    [InlineData(ApplicationStatusEnum.Finished, "Encerrado")]
    [InlineData(ApplicationStatusEnum.Pending, "Recebida")]
    [InlineData(ApplicationStatusEnum.CanceledByCandidate, "Cancelada pelo candidato")]
    public async Task Handle_DeveDescreverOStatusEmPortugues(ApplicationStatusEnum status, string expected)
    {
        GivenProjection(CreateProjection());
        JobApplicationEmailModel? sent = null;
        _emailService
            .Setup(x => x.SendStatusNotificationAsync(It.IsAny<JobApplicationEmailModel>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplicationEmailModel, CancellationToken>((m, _) => sent = m)
            .Returns(Task.CompletedTask);

        await CreateSut().Handle(CreateEvent(newStatus: status), CancellationToken.None);

        sent!.StatusDescription.Should().Be(expected);
    }

    /// <summary>
    /// CA-03: o transporte em baixo não pode virar erro de resposta. A transição já está commitada;
    /// relançar faria o recrutador ver falha numa operação que aconteceu e tentar de novo.
    /// </summary>
    [Fact]
    public async Task Handle_TransporteLanca_NaoDeveRelancar()
    {
        GivenProjection(CreateProjection());
        _emailService
            .Setup(x => x.SendStatusNotificationAsync(It.IsAny<JobApplicationEmailModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP indisponivel"));

        var act = async () => await CreateSut().Handle(CreateEvent(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ConsultaLanca_NaoDeveRelancar()
    {
        _repository
            .Setup(x => x.GetNotificationProjectionAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("banco indisponivel"));

        var act = async () => await CreateSut().Handle(CreateEvent(), CancellationToken.None);

        await act.Should().NotThrowAsync();
        _emailService.Verify(
            x => x.SendStatusNotificationAsync(It.IsAny<JobApplicationEmailModel>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CandidaturaDesapareceuDepoisDoCommit_NaoDeveEnviarNemLancar()
    {
        GivenProjection(null);

        var act = async () => await CreateSut().Handle(CreateEvent(), CancellationToken.None);

        await act.Should().NotThrowAsync();
        _emailService.Verify(
            x => x.SendStatusNotificationAsync(It.IsAny<JobApplicationEmailModel>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Utilizador sem e-mail existe no modelo (o Identity permite-o); tentar enviar para string vazia
    // só produziria uma excepção de transporte por candidatura.
    [Fact]
    public async Task Handle_CandidatoSemEmail_NaoDeveTentarEnviar()
    {
        GivenProjection(CreateProjection(email: string.Empty));

        await CreateSut().Handle(CreateEvent(), CancellationToken.None);

        _emailService.Verify(
            x => x.SendStatusNotificationAsync(It.IsAny<JobApplicationEmailModel>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

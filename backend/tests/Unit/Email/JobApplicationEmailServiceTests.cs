using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.JobApplications.Events;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Email;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Moq;

namespace EmpregaNet.Tests.Unit.Email;

/// <summary>
/// A notificação de andamento <b>não</b> passa pelo teto diário anti-abuso (CA-09).
/// </summary>
/// <remarks>
/// O teto do ADR 0003 existe onde um anónimo consegue disparar volume (<c>forgot-password</c>,
/// <c>resend-confirmation</c>) e onde o descarte silencioso é aceitável. Aqui o descarte silencioso
/// significaria o candidato <b>nunca</b> saber que foi aprovado — o teto estaria a proteger a coisa
/// errada.
/// </remarks>
public sealed class JobApplicationEmailServiceTests
{
    private readonly Mock<IEmailSender> _transport = new();
    private readonly Mock<IEmailThrottleService> _throttle = new();

    private static JobApplicationEmailModel CreateModel(
        JobApplicationNotificationReason reason = JobApplicationNotificationReason.StatusChanged) => new(
        CandidateEmail: "candidato@test.local",
        CandidateName: "Ana",
        JobTitle: "Operador de Empilhadeira",
        CompanyName: "Metalurgica Extrema",
        StatusDescription: "Aprovado",
        NewStatus: ApplicationStatusEnum.Approved,
        OccurredAt: new DateTimeOffset(2026, 9, 2, 12, 30, 0, TimeSpan.Zero),
        ApplicationsUrl: "https://app.test/candidaturas",
        Reason: reason);

    [Fact]
    public async Task SendStatusNotification_DeveEntregarAoDestinatarioComAssuntoDaVaga()
    {
        var sut = new JobApplicationEmailService(_transport.Object);

        await sut.SendStatusNotificationAsync(CreateModel());

        _transport.Verify(
            x => x.SendEmailAsync(
                "candidato@test.local",
                It.Is<string>(s => s.Contains("Operador de Empilhadeira")),
                It.Is<string>(b => b.Contains("Aprovado"))),
            Times.Once);
    }

    [Fact]
    public async Task SendStatusNotification_NaoDeveConsultarOTetoDiarioDeEmails()
    {
        var sut = new JobApplicationEmailService(_transport.Object);

        await sut.SendStatusNotificationAsync(CreateModel());

        _throttle.Verify(x => x.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Reforço estrutural do teste acima: o serviço <b>não tem como</b> consultar o teto, porque não o
    /// recebe. Verificar só <c>Times.Never</c> sobre um mock que ninguém injectou passaria mesmo que
    /// alguém acrescentasse a dependência amanhã — este teste falha nesse momento.
    /// </summary>
    [Fact]
    public void JobApplicationEmailService_NaoDeveDependerDoTetoDiario()
    {
        var dependencies = typeof(JobApplicationEmailService)
            .GetConstructors()
            .SelectMany(c => c.GetParameters())
            .Select(p => p.ParameterType);

        dependencies.Should().NotContain(typeof(IEmailThrottleService));
    }
}

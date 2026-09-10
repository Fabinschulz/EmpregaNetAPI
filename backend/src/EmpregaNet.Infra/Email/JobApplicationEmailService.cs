using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Utils.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace EmpregaNet.Infra.Email;

/// <summary>
/// Notificação de andamento de candidatura: monta o template e delega o transporte a
/// <see cref="IEmailSender"/>, como <see cref="AccountEmailService"/> já faz.
/// </summary>
public sealed class JobApplicationEmailService : IJobApplicationEmailService
{
    private readonly IEmailSender _transport;

    public JobApplicationEmailService(IEmailSender transport)
    {
        _transport = transport;
    }

    /// <inheritdoc />
    /// <remarks>
    /// O token é verificado <b>antes</b> do envio e não durante: <c>IEmailSender.SendEmailAsync</c> não
    /// aceita <c>CancellationToken</c>, portanto, após entregar a mensagem ao transporte, a operação
    /// já não pode ser cancelada. Essa checagem evita enviar e-mail quando a requisição foi abandonada,
    /// que é o cenário relevante para este caso.
    /// </remarks>
    public Task SendStatusNotificationAsync(
        JobApplicationEmailModel model,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (subject, html) = EmpregaNetEmailTemplates.JobApplicationStatus(
            candidateName: model.CandidateName,
            jobTitle: model.JobTitle,
            companyName: model.CompanyName,
            statusDescription: model.StatusDescription,
            newStatus: model.NewStatus,
            occurredAt: BrasiliaTime.Format(model.OccurredAt),
            applicationsLink: model.ApplicationsUrl,
            reason: model.Reason);

        return _transport.SendEmailAsync(model.CandidateEmail, subject, html);
    }
}

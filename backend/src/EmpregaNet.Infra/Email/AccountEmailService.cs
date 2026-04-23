using EmpregaNet.Application.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace EmpregaNet.Infra.Email;

/// <summary>
/// Implementação de e-mails de conta: delega o transporte a <see cref="IEmailSender"/> (NoOp, SMTP, SendGrid, etc.).
/// </summary>
public sealed class AccountEmailService : IAccountEmailService
{
    private readonly IEmailSender _transport;

    public AccountEmailService(IEmailSender transport)
    {
        _transport = transport;
    }

    public Task SendPasswordResetLinkAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var (subject, html) = EmpregaNetEmailTemplates.PasswordReset(resetLink);
        return _transport.SendEmailAsync(toEmail, subject, html);
    }

    public Task SendEmailConfirmationLinkAsync(string toEmail, string confirmationLink, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var (subject, html) = EmpregaNetEmailTemplates.EmailConfirmation(confirmationLink);
        return _transport.SendEmailAsync(toEmail, subject, html);
    }
}

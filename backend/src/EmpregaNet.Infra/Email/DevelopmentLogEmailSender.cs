using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Infra.Email;

/// <summary>
/// Transporte de desenvolvimento: não entrega nada, escreve destinatário, assunto e corpo no log.
/// </summary>
public sealed class DevelopmentLogEmailSender : IEmailSender
{
    private readonly ILogger<DevelopmentLogEmailSender> _logger;

    public DevelopmentLogEmailSender(ILogger<DevelopmentLogEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("[E-MAIL DEV] Para: {Email} | Assunto: {Subject}", email, subject);
        _logger.LogDebug("[E-MAIL DEV] Corpo para {Email}:{NewLine}{HtmlMessage}", email, Environment.NewLine, htmlMessage);

        return Task.CompletedTask;
    }
}

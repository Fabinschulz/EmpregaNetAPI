using EmpregaNet.Application.Auth.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmpregaNet.Infra.Email;

/// <summary>
/// Transporte SMTP para <see cref="IEmailSender"/> (reset de senha e outros e-mails transacionais).
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpEmailOptions _opt;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpEmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _opt = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_opt.FromName, _opt.FromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

        try
        {
            using var client = new SmtpClient();

            var secure = ParseSecurity(_opt.Security);
            await client.ConnectAsync(_opt.Host, _opt.Port, secure);

            if (!string.IsNullOrEmpty(_opt.UserName))
                await client.AuthenticateAsync(_opt.UserName, _opt.Password ?? string.Empty);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("E-mail SMTP enviado para {Email} assunto {Subject}.", email, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha SMTP ao enviar para {Email} assunto {Subject}.", email, subject);
            throw new InvalidOperationException(
                "Não foi possível enviar o e-mail. Verifique Smtp:Host, porta, Security e credenciais.",
                ex);
        }
    }

    private static SecureSocketOptions ParseSecurity(string? mode)
    {
        return mode?.Trim() switch
        {
            "SslOnConnect" or "Implicit" or "465" => SecureSocketOptions.SslOnConnect,
            "None" or "Off" => SecureSocketOptions.None,
            _ => SecureSocketOptions.StartTls
        };
    }
}

using System.Text;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using EmpregaNet.Application.Auth.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Infra.Email;

/// <summary>
/// Transporte Amazon SES para <see cref="IEmailSender"/> reset de senha, confirmação de conta
/// e notificações de andamento de candidatura.
/// </summary>
public sealed class SesEmailSender : IEmailSender
{
    private readonly IAmazonSimpleEmailServiceV2 _ses;
    private readonly SesEmailOptions _opt;
    private readonly ILogger<SesEmailSender> _logger;

    public SesEmailSender(
        IAmazonSimpleEmailServiceV2 ses,
        IOptions<SesEmailOptions> options,
        ILogger<SesEmailSender> logger)
    {
        _ses = ses;
        _opt = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var request = new SendEmailRequest
        {
            FromEmailAddress = FormatSender(_opt.FromName, _opt.FromEmail),
            Destination = new Destination { ToAddresses = [email] },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content { Data = subject, Charset = "UTF-8" },
                    Body = new Body { Html = new Content { Data = htmlMessage, Charset = "UTF-8" } }
                }
            }
        };

        try
        {
            var response = await _ses.SendEmailAsync(request);

            _logger.LogInformation(
                "E-mail SES enviado para {Email} assunto {Subject} MessageId {MessageId}.",
                email, subject, response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha SES ao enviar para {Email} assunto {Subject}.", email, subject);
            throw new InvalidOperationException(DiagnosticMessage(ex), ex);
        }
    }

    private static string DiagnosticMessage(Exception ex) => ex switch
    {
        NotFoundException =>
            "SES: a identidade do remetente (Ses:FromEmail) não existe na região configurada em Ses:Region. " +
            "A verificação é por região - confirme que é a mesma onde o domínio/endereço foi verificado.",

        MessageRejectedException =>
            "SES recusou a mensagem. Causa habitual: a conta ainda está no sandbox do SES, onde só é " +
            "possível enviar para endereços verificados. Peça acesso de produção ou verifique o destinatário.",

        AccountSuspendedException or SendingPausedException =>
            "SES: o envio está suspenso ou pausado para esta conta/configuração (em geral por taxa de " +
            "bounce ou complaint acima do limite). Resolva no console do SES antes de reenviar.",

        Amazon.Runtime.AmazonServiceException =>
            "Não foi possível enviar o e-mail pelo SES. Verifique Ses:Region, Ses:FromEmail e se a role " +
            "da instância tem permissão ses:SendEmail.",

        _ => "Não foi possível enviar o e-mail pelo SES."
    };

    /// <summary>
    /// Monta o cabeçalho <c>From:</c> no formato <c>"Nome" &lt;endereço&gt;</c>. Nome com caracteres
    /// não-ASCII é codificado em RFC 2047, porque o SES exige o cabeçalho já codificado, sem isto um
    /// <c>FromName</c> acentuado chega truncado ou recusado.
    /// </summary>
    private static string FormatSender(string? name, string address)
    {
        if (string.IsNullOrWhiteSpace(name))
            return address;

        var displayName = name.All(char.IsAscii)
            ? $"\"{name.Replace("\\", "\\\\").Replace("\"", "\\\"")}\""
            : $"=?UTF-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(name))}?=";

        return $"{displayName} <{address}>";
    }
}

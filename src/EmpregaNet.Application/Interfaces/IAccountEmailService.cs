namespace EmpregaNet.Application.Interfaces;

/// <summary>
/// Envio de e-mails transacionais da aplicação (reset de senha, etc.).
/// </summary>
public interface IAccountEmailService
{
    /// <summary>Envia o e-mail com o link para redefinir a senha.</summary>
    Task SendPasswordResetLinkAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);

    /// <summary>Envia o e-mail com o link para confirmar o endereço de e-mail após o registo.</summary>
    Task SendEmailConfirmationLinkAsync(string toEmail, string confirmationLink, CancellationToken cancellationToken = default);
}

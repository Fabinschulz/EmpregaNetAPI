namespace EmpregaNet.Application.Interfaces;

/// <summary>
/// Envio de e-mails transacionais da aplicação (reset de senha, etc.).
/// </summary>
public interface IAccountEmailService
{
    /// <summary>Envia o e-mail com o link para redefinir a senha (conteúdo já com identidade EmpregaNet).</summary>
    Task SendPasswordResetLinkAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);
}

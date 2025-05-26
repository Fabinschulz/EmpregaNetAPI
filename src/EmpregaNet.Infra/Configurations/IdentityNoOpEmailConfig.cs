using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace EmpregaNet.Infra.Configurations
{
    // OBS: Essa classe é um NoOp (No Operation) para o envio de e-mails. Ela não envia e-mails, apenas simula o envio: utilizada para testes e desenvolvimento.
    internal sealed class IdentityNoOpEmailSender : IEmailSender<User>
    {
        private readonly IEmailSender emailSender = new NoOpEmailSender();

        public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink) =>
             emailSender.SendEmailAsync(email, "Confirme seu e-mail",
                $"Clique <a href='{confirmationLink}'>aqui</a> para confirmar sua conta.");
        public Task SendPasswordResetLinkAsync(User user, string email, string resetLink) =>
            emailSender.SendEmailAsync(email, "Redefinir senha",
                $"Clique <a href='{resetLink}'>aqui</a> para redefinir sua senha.");
        public Task SendPasswordResetCodeAsync(User user, string email, string resetCode) =>
            emailSender.SendEmailAsync(email, "Redefinir senha",
                $"Use o seguinte código para redefinir sua senha: {resetCode}.");
    }
}
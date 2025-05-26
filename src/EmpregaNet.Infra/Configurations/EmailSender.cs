using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace EmpregaNet.Infra.Configurations
{
    public class EmailSender : IEmailSender<User>
    {
        private readonly IEmailSender _emailSender;

        public EmailSender(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink) =>
            _emailSender.SendEmailAsync(email, "Confirme seu e-mail",
                $"Clique <a href='{confirmationLink}'>aqui</a> para confirmar sua conta.");

        public Task SendPasswordResetLinkAsync(User user, string email, string resetLink) =>
            _emailSender.SendEmailAsync(email, "Redefinir senha",
                $"Clique <a href='{resetLink}'>aqui</a> para redefinir sua senha.");

        public Task SendPasswordResetCodeAsync(User user, string email, string resetCode) =>
            _emailSender.SendEmailAsync(email, "Redefinir senha",
                $"Use o seguinte código para redefinir sua senha: {resetCode}.");

        public Task SendEmailAsync(string email, string subject, string htmlMessage) =>
            _emailSender.SendEmailAsync(email, subject, htmlMessage);
    }
}
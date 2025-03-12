using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Configurations
{
    public static class RegisterServicesConfiguration
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IEmailSender<User>, NullEmailSender>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }


        public class NullEmailSender : IEmailSender<User>
        {
            public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink) => Task.CompletedTask;
            public Task SendPasswordResetLinkAsync(User user, string email, string resetLink) => Task.CompletedTask;
            public Task SendPasswordResetCodeAsync(User user, string email, string resetCode) => Task.CompletedTask;
        }
    }
}

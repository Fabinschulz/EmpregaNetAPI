using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace EmpregaNet.Infra.Configurations
{
    public static class ApiConfig
    {

        public static void UseApiConfiguration(this WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.ApplyMigrations();
            }

            // Under certain scenarios, e.g minikube / linux environment / behind load balancer
            // https redirection could lead dev's to over complicated configuration for testing purpouses
            // In production is a good practice to keep it true
            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI();

            // Middleware de autenticação e autorização
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            // app.MapIdentityApi<User>();

        }
    }
}
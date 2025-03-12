using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra
{
    public static class DependencyInjection
    {
        public static void AddWebApplication(this WebApplicationBuilder builder)
        {
            builder.AddSwaggerDoc();
            builder.AddDatabase();
        }

        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddOpenApi();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.ConfigureCorsPolicy();
            services.AddIdentityConfiguration();
            services.RegisterServices();
        }
    }
}
using EmpregaNet.Infra.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
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

        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenApi();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.ConfigureCorsPolicy();
            services.AddIdentityConfiguration();
            services.RegisterServices();
            services.Configure<ElasticsearchSettings>(configuration.GetSection("ElasticsearchSettings"));
        }
    }
}
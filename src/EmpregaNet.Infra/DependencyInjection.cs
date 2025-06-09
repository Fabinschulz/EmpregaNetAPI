using System.Text.Json.Serialization;
using EmpregaNet.Infra.Cache.ElastiCacheRedis;
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
            builder.AddDatabase();
            builder.AddIdentityConfiguration();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            // services.Configure<ElasticsearchSettings>(configuration.GetSection("ElasticsearchSettings"));
        }

        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.UseRedisCache(configuration);
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddEndpointsApiExplorer();
            services.DIRegistrationServices();
            services.AddProblemDetails();
            services.AddSwaggerGen();
        }
    }
}
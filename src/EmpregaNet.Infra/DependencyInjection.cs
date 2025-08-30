using EmpregaNet.Infra.Cache.ElastiCacheRedis;
using EmpregaNet.Infra.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra
{
    public static class DependencyInjection
    {
        public static void RegisterInfraDependency(this WebApplicationBuilder builder)
        {
            builder.AddDatabase();
            builder.AddIdentityConfiguration();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            builder.Services.ConfigureServices(builder.Configuration);
            builder.AddSentryConfiguration();
        }

        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.UseRedisCache(configuration);
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddEndpointsApiExplorer();
            services.RegisterRepositories();
            services.AddProblemDetails();
            services.AddSwaggerGen();
        }
    }
}
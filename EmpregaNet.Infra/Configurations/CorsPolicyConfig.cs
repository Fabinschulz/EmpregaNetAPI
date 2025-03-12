using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Configurations
{
    public static class CorsPolicyConfig
    {
        /// <summary>
        /// Configures the CORS policy to allow any origin, method, and header.
        /// </summary>
        public static IServiceCollection ConfigureCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(opt =>
            {
                opt.AddDefaultPolicy(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            return services;
        }
    }
}

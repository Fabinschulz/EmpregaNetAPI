using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Configurations
{
    // Permitir requisições de qualquer origem
    /// <summary>
    /// Provides extension methods for configuring CORS policies.
    /// </summary>
    /// <remarks>
    /// This class is used to configure the CORS policy to allow any origin, method, and header.
    /// </remarks>
    public static class CorsPolicyConfig
    {
        /// <summary>
        /// Configures the CORS policy to allow any origin, method, and header.
        /// </summary>
        public static void ConfigureCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(opt =>
            {
                opt.AddDefaultPolicy(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
        }
    }
}

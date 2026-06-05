using EmpregaNet.Application.Auth.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Api.Configuration;

public static class CorsPolicyConfig
{
    public const string DefaultPolicyName = "EmpregaNetCors";

    /// <summary>
    /// CORS restrito a <c>AppUrls:CorsAllowedOrigins</c>. Credenciais permitidas para cookies de auth.
    /// </summary>
    public static IServiceCollection ConfigureCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppUrlsOptions>(configuration.GetSection(AppUrlsOptions.SectionName));

        var appUrls = configuration.GetSection(AppUrlsOptions.SectionName).Get<AppUrlsOptions>()
            ?? new AppUrlsOptions();

        var origins = appUrls.ResolveCorsOrigins();

        services.AddCors(opt =>
        {
            opt.AddPolicy(DefaultPolicyName, builder => builder
                .WithOrigins(origins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });

        return services;
    }
}

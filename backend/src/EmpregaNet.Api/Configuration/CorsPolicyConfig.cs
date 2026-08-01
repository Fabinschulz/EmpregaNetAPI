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
    public static IServiceCollection ConfigureCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<AppUrlsOptions>(configuration.GetSection(AppUrlsOptions.SectionName));

        var section = configuration.GetSection(AppUrlsOptions.SectionName);
        var appUrls = section.Get<AppUrlsOptions>() ?? new AppUrlsOptions();

        var configured = appUrls.CorsAllowedOrigins?.Any(o => !string.IsNullOrWhiteSpace(o)) == true
                         || !string.IsNullOrWhiteSpace(appUrls.PublicAppBaseUrl);

        if (!configured && !environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                $"'{AppUrlsOptions.SectionName}:{nameof(AppUrlsOptions.CorsAllowedOrigins)}' precisa listar ao menos " +
                "uma origem fora de Development. Configure via variável de ambiente " +
                $"({AppUrlsOptions.SectionName}__{nameof(AppUrlsOptions.CorsAllowedOrigins)}__0) ou no appsettings do ambiente.");
        }

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

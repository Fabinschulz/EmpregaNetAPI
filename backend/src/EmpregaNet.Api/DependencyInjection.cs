using System.Text.Json.Serialization;
using EmpregaNet.Api.Controllers.HealthChecks;
using EmpregaNet.Api.Configuration;
using EmpregaNet.Application.Auth;
using EmpregaNet.Infra.Cache;
using EmpregaNet.Infra.Extensions;
using Newtonsoft.Json;

public static class DependencyInjection
{

    public static void SetupApiServices(this WebApplication app)
    {
        app.UseHttpsRedirection()
           .UseCors(CorsPolicyConfig.DefaultPolicyName)
           .UseAuthentication()
           .UseAuthorization()
           .UseOutputCache()
           .UseSwaggerSetup()
           .Use(async (context, next) =>
                {
                    // Extrai claims do usuário e adiciona como headers na requisição
                    HttpUserContext.SetHeader(context);

                    await next.Invoke();
                });

        app.MapControllers()
           .RequireRateLimiting(RateLimit.PolicyName);
        app.MapHealthChecks("/health");
        // app.MapIdentityApi<Usuario>();

    }

    public static IServiceCollection RegisterApiDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        })
        .AddNewtonsoftJson(options =>
        {
            // Ignora referências circulares durante a serialização JSON
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            //options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; // Se descomentado, ignora propriedades nulas no JSON de resposta
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.SetupSwaggerDocumentation();
        services.ConfigureCorsPolicy(configuration);
        services.SetupOutputCache(configuration);
        services.AddSingleton<AuthCookieService>();
        services.AddScoped<HttpUserContext>();
        services.AddScoped<IHttpCurrentUser, HttpCurrentUser>();

        var healthChecks = services.AddHealthChecks()
            .AddCheck<DatabaseCheck>("Database");

        if (RedisOptions.Resolve(configuration).IsActive)
            healthChecks.AddCheck<RedisHealthCheck>("Redis");

        return services;
    }
}

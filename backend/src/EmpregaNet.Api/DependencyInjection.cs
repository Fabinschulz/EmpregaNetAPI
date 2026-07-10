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
        // HSTS apenas fora de Development (evita fixar HSTS em http://localhost).
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        // Headers de segurança em todas as respostas.
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["X-Permitted-Cross-Domain-Policies"] = "none";

            // A API responde JSON; bloqueia scripts/embedding. O Swagger UI precisa de CSP própria.
            if (!context.Request.Path.StartsWithSegments("/swagger"))
            {
                headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";
            }

            await next.Invoke();
        });

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

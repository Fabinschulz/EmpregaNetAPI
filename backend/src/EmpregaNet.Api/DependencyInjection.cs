using System.Text.Json.Serialization;
using EmpregaNet.Api.Controllers.HealthChecks;
using EmpregaNet.Api.Configuration;
using EmpregaNet.Api.Middleware;
using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Auth.UseCase;
using EmpregaNet.Infra.Cache;
using Microsoft.AspNetCore.ResponseCompression;

public static class DependencyInjection
{

    public static void SetupApiServices(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseResponseCompression();

        // Configura o uso de headers de proxy reverso (X-Forwarded-For, X-Forwarded-Proto) caso esteja atrás de um proxy reverso.
        app.UseForwardedHeadersIfConfigured();

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
           .UseRateLimiter()
           .UseAuthorization()
           .UseOutputCache()
           .UseSwaggerSetup()
           .Use(async (context, next) =>
                {
                    // Extrai claims do usuário e adiciona como headers na requisição
                    HttpUserContext.SetHeader(context);

                    await next.Invoke();
                });


        app.MapControllers();
        app.MapHealthCheckEndpoints();
    }

    public static IServiceCollection RegisterApiDependencies(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        })
        // `AddNewtonsoftJson` substitui os formatters do MVC: é o Newtonsoft, e não o
        // System.Text.Json, que serializa tudo o que sai dos controllers. Qualquer regra de
        // serialização das respostas precisa de ser configurada aqui.
        .AddNewtonsoftJson(ResponseJsonConfig.Configure)
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.SetupSwaggerDocumentation();
        services.ConfigureCorsPolicy(configuration, environment);
        services.SetupOutputCache(configuration);
        services.AddSingleton<AuthCookieService>();

        // Respostas JSON, muitas delas listagens paginadas: o ganho de banda é direto.
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        services.AddScoped<HttpUserContext>();
        services.AddScoped<IHttpCurrentUser, HttpCurrentUser>();

        // Tag 'ready': dependências externas, que dizem se a instância pode receber tráfego.
        var healthChecks = services.AddHealthChecks()
            .AddCheck<DatabaseCheck>("Database", tags: [HealthCheckConfig.ReadinessTag]);

        if (RedisOptions.Resolve(configuration).IsActive)
            healthChecks.AddCheck<RedisHealthCheck>("Redis");

        return services;
    }
}

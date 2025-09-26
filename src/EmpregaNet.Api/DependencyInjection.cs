using System.Text.Json.Serialization;
using EmpregaNet.Api.Configuration;
using EmpregaNet.Application.Service.Auth;
using Newtonsoft.Json;

public static class DependencyInjection
{

    public static void SetupApiServices(this WebApplication app)
    {
        app.UseHttpsRedirection()
           .UseCors("AllowAll")
           .UseAuthentication()
           .UseAuthorization()
           .UseRouting()
           .UseSwaggerSetup()
           .Use(async (context, next) =>
                {
                    // Extrai claims do usuário e adiciona como headers na requisição
                    HttpUserContext.SetHeader(context);

                    await next.Invoke();
                });

        app.MapControllers();
        app.MapHealthChecks("/health");
        // app.MapIdentityApi<Usuario>();

    }

    public static IServiceCollection RegisterApiDependencies(this IServiceCollection services)
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
        services.ConfigureCorsPolicy();
        services.AddScoped<HttpUserContext>();
        services.AddScoped<IHttpCurrentUser, HttpCurrentUser>();

        services.AddHealthChecks()
                       .AddCheck<DatabaseCheck>("Database")
                       .AddCheck<MemoryServiceCheck>("Cache");

        return services;
    }
}

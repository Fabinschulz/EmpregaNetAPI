using System.Text.Json.Serialization;
using EmpregaNet.Application.Service;
using EmpregaNet.Infra.Configurations;
using Newtonsoft.Json;

namespace EmpregaNet.Api.Configurations
{

    public static class ApiConfig
    {

        public static void UseApiConfiguration(this WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.ApplyMigrations();
            }

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
            // app.MapIdentityApi<Usuario>();

        }

        public static WebApplicationBuilder AddApiConfiguration(this WebApplicationBuilder builder)
        {
            builder.EnvironmentConfig();
            builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                // Ignora referências circulares durante a serialização JSON
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; // Se descomentado, ignora propriedades nulas no JSON de resposta
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            }); ;

            builder.AddSwaggerConfiguration();
            builder.Services.ConfigureCorsPolicy();
            builder.Services.AddScoped<HttpUserContext>();
            builder.Services.AddScoped<IHttpCurrentUser, HttpCurrentUser>();

            builder.Services.AddHealthChecks()
                           .AddCheck<DatabaseCheck>("Database")
                           .AddCheck<MemoryServiceCheck>("Cache");

            return builder;
        }
    }
}
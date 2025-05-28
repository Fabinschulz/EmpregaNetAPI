using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmpregaNet.Infra.Configurations
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
               .UseCors(c =>
               {
                   c.AllowAnyHeader();
                   c.AllowAnyMethod();
                   c.AllowAnyOrigin();
               })
               .UseAuthentication()
               .UseAuthorization();

            // Middleware de autenticação e autorização
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseSwaggerSetup();
            // app.MapIdentityApi<Usuario>();

        }

        public static WebApplicationBuilder AddApiConfiguration(this WebApplicationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Configuration
                        .SetBasePath(builder.Environment.ContentRootPath)
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
                        .AddEnvironmentVariables();

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            }); ;

            builder.AddSwaggerConfiguration();

            return builder;
        }
    }
}
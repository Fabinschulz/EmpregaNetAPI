using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EmpregaNet.Infra.Configurations
{
    public static class SwaggerConfig
    {
        public static WebApplicationBuilder AddSwaggerDoc(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new()
                {
                    Title = "EmpregaNet API",
                    Version = "v1",
                    Description = "Uma API para cadastro de vagas de emprego",
                    License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/Licenses/MIT") },
                    Contact = new OpenApiContact() { Name = "Fabio Lima", Email = "fabio.lima19997@gmail.com" },
                });

                config.TagActionsBy(api => new[] { api.GroupName ?? "Default" });
                config.DocInclusionPredicate((name, api) => true);
                config.EnableAnnotations();
                config.DocumentFilter<TagDescriptionsDocumentFilter>();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                config.IncludeXmlComments(xmlPath);
            });

            return builder;
        }

        public class TagDescriptionsDocumentFilter : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                swaggerDoc.Tags = new List<OpenApiTag>
            {
                new OpenApiTag { Name = "Default", Description = "Endpoints relacionados a autenticação" },
            };
            }
        }
    }
}
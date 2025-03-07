using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EmpregaNet.Infra.Configurations
{
    public static class SwaggerConfig
    {
        public static void AddSwaggerDoc(this WebApplicationBuilder builder)
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
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                config.IncludeXmlComments(xmlPath);
                config.DocumentFilter<TagDescriptionsDocumentFilter>();
            });
        }

        public class TagDescriptionsDocumentFilter : IDocumentFilter
        {
            /// <summary>
            /// Applies the filter to the OpenApiDocument.
            /// </summary>
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                swaggerDoc.Tags = new List<OpenApiTag>
                {
                    new OpenApiTag { Name = "EmpregaNet", Description = "EmpregaNet API" }
                };
            }
        }


    }
}
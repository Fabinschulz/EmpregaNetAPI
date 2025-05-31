using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;


namespace EmpregaNet.Infra.Configurations
{
    public static class SwaggerConfig
    {
        public static WebApplicationBuilder AddSwaggerConfiguration(this WebApplicationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Authentication and Authorization API",
                    Description = "OAuth API Swagger surface",
                    Contact = new OpenApiContact { Name = "Freetech", Email = "freetech@outlook.com.br", Url = new Uri("https://freetech.vercel.app/") },
                    License = new OpenApiLicense { Name = "MIT" }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                s.IncludeXmlComments(xmlPath);
                s.DocumentFilter<TagDescriptionsDocumentFilter>();

                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Input the JWT like: Bearer {your token}",
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                s.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                // Excluding ASP.NET Identity endpoints
                s.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var relativePath = apiDesc.RelativePath;

                    var identityEndpoints = new[]
                    {
                        "register",
                        "manage",
                        "refresh",
                        "login",
                        "confirmEmail",
                        "resendConfirmationEmail",
                        "forgotPassword",
                        "resetPassword"
                    };

                    // Validating if the endpoint is avoided
                    foreach (var endpoint in identityEndpoints)
                    {
                        if (relativePath!.Contains(endpoint, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }

                    return true;
                });

            });

            return builder;
        }

        public class TagDescriptionsDocumentFilter : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                swaggerDoc.Tags = new List<OpenApiTag>
                {
                    new OpenApiTag { Name = "USER", Description = "User API" }
                };
            }
        }

        public static IApplicationBuilder UseSwaggerSetup(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });

            return app;
        }
    }
}
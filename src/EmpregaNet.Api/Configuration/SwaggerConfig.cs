using System.Reflection;
using System.Xml.Linq;
using EmpregaNet.Infra.Utils;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;


namespace EmpregaNet.Api.Configurations
{
    public static class SwaggerConfig
    {
        /// <summary>
        /// Adiciona e configura o Swagger/OpenAPI no pipeline de serviços da aplicação.
        /// Inclui documentação, segurança JWT, filtros de schema e comentários XML de múltiplos projetos.
        /// </summary>
        /// <param name="builder">Coleção de serviços da aplicação (<see cref="WebApplicationBuilder"/>).</param>
        /// <returns>Coleção de serviços configurada.</returns>
        public static WebApplicationBuilder AddSwaggerConfiguration(this WebApplicationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddEndpointsApiExplorer();

            // Define informações básicas da documentação Swagger
            builder.Services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Authentication and Authorization API",
                    Description = "EmpregaNet API Swagger surface",
                    Contact = new OpenApiContact { Name = "Freetech", Email = "freetech@outlook.com.br", Url = new Uri("https://freetech.vercel.app/") },
                    License = new OpenApiLicense { Name = "MIT" }
                });

                // Configura autenticação JWT Bearer no Swagger
                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Insira o token JWT desta maneira: Bearer {seu token}",
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                // Exige o uso do esquema de segurança Bearer em todas as operações
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

                // Suporte a polimorfismo em schemas usando oneOf
                s.UseOneOfForPolymorphism();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                s.IncludeXmlComments(xmlPath);
                s.DocumentFilter<TagDescriptionsDocumentFilter>();
                s.SchemaFilter<DescribeEnumMembers>(XDocument.Load(xmlPath));
                s.SchemaFilter<IgnoreEnumSchemaFilter>(XDocument.Load(xmlPath));

                var appName = "EmpregaNet";
                // Inclui comentários e filtros do projeto Application
                xmlPath = Path.Combine(AppContext.BaseDirectory, $"{appName}.Application.xml");
                s.IncludeXmlComments(xmlPath);
                s.SchemaFilter<DescribeEnumMembers>(XDocument.Load(xmlPath));
                s.SchemaFilter<IgnoreEnumSchemaFilter>(XDocument.Load(xmlPath));

                // Inclui comentários e filtros do projeto Domain
                xmlPath = Path.Combine(AppContext.BaseDirectory, $"{appName}.Domain.xml");
                s.IncludeXmlComments(xmlPath);
                s.SchemaFilter<DescribeEnumMembers>(XDocument.Load(xmlPath));
                s.SchemaFilter<IgnoreEnumSchemaFilter>(XDocument.Load(xmlPath));

            });

            return builder;
        }

        public class TagDescriptionsDocumentFilter : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                swaggerDoc.Tags = new List<OpenApiTag>
                {
                    new OpenApiTag {
                        Name = "Users",
                        Description = "Endpoints para gestão de autenticação e autorização"
                    },
                    new OpenApiTag {
                        Name = "Jobs",
                        Description = "Endpoints para gerenciamento de oportunidades de emprego"
                    },
                    new OpenApiTag {
                        Name = "Candidates",
                        Description = "Endpoints para administração de cadastros de candidatos"
                    },
                    new OpenApiTag {
                        Name = "Companies",
                        Description = "Endpoints para administração de cadastros corporativos"
                    },
                    new OpenApiTag {
                        Name = "JobApplications",
                        Description = "Endpoints para processamento de inscrições em vagas"
                    },
                    new OpenApiTag {
                        Name = "Search",
                        Description = "Endpoints para consultas avançadas de vagas e empresas"
                    },
                    new OpenApiTag {
                        Name = "Notifications",
                        Description = "Endpoints para gestão de alertas e comunicações"
                    },
                    new OpenApiTag {
                        Name = "Admin",
                        Description = "Endpoints privilegiados para gestão do sistema"
                    }
                };
            }
        }

        public static IApplicationBuilder UseSwaggerSetup(this IApplicationBuilder app)
        {

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelsExpandDepth(-1);
            });

            return app;
        }
    }
}
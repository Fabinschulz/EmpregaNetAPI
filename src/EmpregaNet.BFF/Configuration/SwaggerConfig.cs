using System.Reflection;
using System.Xml.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public static class SwaggerConfig
{
    /// <summary>
    /// Adiciona e configura o Swagger/OpenAPI no pipeline de serviços da aplicação.
    /// Inclui documentação, segurança JWT, filtros de schema e comentários XML de múltiplos projetos.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação (<see cref="IServiceCollection"/>).</param>
    /// <returns>Coleção de serviços configurada.</returns>
    public static IServiceCollection SetupSwaggerDocumentation(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.AddEndpointsApiExplorer();

        // Define informações básicas da documentação Swagger
        services.AddSwaggerGen(s =>
        {
            s.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "BFF EmpregaNet",
                Description = "Serviço BFF (Backend For Frontend) do EmpregaNet.",
                Contact = new OpenApiContact { Name = "EmpregaNet", Email = "contato@empreganet.com.br", Url = new Uri("https://empreganet.com.br/") },
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

                // Insira na str vazia os caminhos que deseja excluir
                return !(relativePath?.StartsWith(" ", StringComparison.OrdinalIgnoreCase) ?? false);
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

        return services;
    }

    public class TagDescriptionsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = new List<OpenApiTag>
                {
                    //new OpenApiTag { Name = "Atendimentos", Description = "Operações relacionadas a atendimentos." },
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

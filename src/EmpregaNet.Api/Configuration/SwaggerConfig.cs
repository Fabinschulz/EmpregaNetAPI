using System.Reflection;
using System.Xml.Linq;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EmpregaNet.Api.Configuration
{
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
                    Title = "Gerenciamento de Vagas de Emprego - EmpregaNet",
                    Description = "EmpregaNet API Swagger surface",
                    Contact = new OpenApiContact { Name = "Freetech", Email = "freetech@outlook.com.br", Url = new Uri("https://freetech.vercel.app/") },
                    License = new OpenApiLicense { Name = "MIT" }
                });

                // Configura autenticação JWT Bearer no Swagger
                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Insira o token JWT desta maneira: {seu token}",
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http
                });

                s.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });

                // Excluding ASP.NET Identity endpoints
                // s.DocInclusionPredicate((docName, apiDesc) =>
                //  {
                //      var relativePath = apiDesc.RelativePath;

                //      return !(relativePath?.StartsWith("Identity", StringComparison.OrdinalIgnoreCase) ?? false);
                //  });

                // Suporte a polimorfismo em schemas usando oneOf
                s.UseOneOfForPolymorphism();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    s.IncludeXmlComments(xmlPath);
                    var xmlDoc = XDocument.Load(xmlPath);
                    s.SchemaFilter<DescribeEnumMembers>(xmlDoc);
                }

                s.DocumentFilter<TagDescriptionsDocumentFilter>();
                s.SchemaFilter<IgnoreEnumSchemaFilter>();

                var appName = "EmpregaNet";
                xmlPath = Path.Combine(AppContext.BaseDirectory, $"{appName}.Application.xml");
                if (File.Exists(xmlPath))
                {
                    s.IncludeXmlComments(xmlPath);
                    var xmlDoc = XDocument.Load(xmlPath);
                    s.SchemaFilter<DescribeEnumMembers>(xmlDoc);
                }
                s.SchemaFilter<IgnoreEnumSchemaFilter>();

                xmlPath = Path.Combine(AppContext.BaseDirectory, $"{appName}.Domain.xml");
                if (File.Exists(xmlPath))
                {
                    s.IncludeXmlComments(xmlPath);
                    var xmlDoc = XDocument.Load(xmlPath);
                    s.SchemaFilter<DescribeEnumMembers>(xmlDoc);
                }
                s.SchemaFilter<IgnoreEnumSchemaFilter>();

            });

            return services;
        }

        public class TagDescriptionsDocumentFilter : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                swaggerDoc.Tags = new HashSet<OpenApiTag>
                {
                    new() { Name = "Users", Description = "Endpoints para gestão de autenticação e autorização" },
                    new() { Name = "Jobs", Description = "Endpoints para gerenciamento de oportunidades de emprego" },
                    new() { Name = "Candidates", Description = "Endpoints para administração de cadastros de candidatos" },
                    new() { Name = "Companies", Description = "Endpoints para administração de cadastros corporativos" },
                    new() { Name = "JobApplications", Description = "Endpoints para processamento de inscrições em vagas" },
                    new() { Name = "Search", Description = "Endpoints para consultas avançadas de vagas e empresas" },
                    new() { Name = "Notifications", Description = "Endpoints para gestão de alertas e comunicações" },
                    new() { Name = "Admin", Description = "Endpoints privilegiados para gestão do sistema" }
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
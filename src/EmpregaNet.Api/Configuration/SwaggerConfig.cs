using System.Reflection;
using System.Xml.Linq;
using EmpregaNet.Application.Utils;
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
                s.SwaggerDoc(Constants.OpenApi.V1, new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Gerenciamento de Vagas de Emprego",
                    Description = "Superfície completa: conta, vagas, candidaturas e documentação agrupada por tags.",
                    Contact = new OpenApiContact { Name = "Freetech", Email = "freetech@outlook.com.br", Url = new Uri("https://freetech.vercel.app/") },
                    License = new OpenApiLicense { Name = "MIT" }
                });

                // s.SwaggerDoc(Constants.OpenApi.Admin, new OpenApiInfo
                // {
                //     Version = "v1",
                //     Title = "EmpregaNet — Administração",
                //     Description = "Somente endpoints privilegiados (políticas Administrador e fluxos de recrutamento alinhados ao grupo OpenAPI admin).",
                //     Contact = new OpenApiContact { Name = "Freetech", Email = "freetech@outlook.com.br", Url = new Uri("https://freetech.vercel.app/") },
                //     License = new OpenApiLicense { Name = "MIT" }
                // });

                // v1: superfície geral (conta, vagas, candidaturas, etc.). admin: somente [ApiExplorerSettings(GroupName = admin)].
                // Server para ocultar endpoints administrativos do documento público (v1) e vice-versa, usando o GroupName definido nos controllers e ações.
                // s.DocInclusionPredicate((docName, apiDesc) =>
                // {
                //     var isAdminSurface = string.Equals(apiDesc.GroupName, Constants.OpenApi.Admin, StringComparison.OrdinalIgnoreCase);

                //     if (string.Equals(docName, Constants.OpenApi.V1, StringComparison.OrdinalIgnoreCase))
                //         return !isAdminSurface;

                //     if (string.Equals(docName, Constants.OpenApi.Admin, StringComparison.OrdinalIgnoreCase))
                //         return isAdminSurface;

                //     return false;
                // });

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
                    new() { Name = "Users", Description = "Conta do usuário: registro, login." },
                    new() { Name = "Jobs", Description = "Oportunidades de emprego (leitura pública; mutações com política de recrutamento)." },
                    new() { Name = "Candidates", Description = "Listagem e detalhe de candidatos (equipe de recrutamento)." },
                    new() { Name = "JobApplications", Description = "Candidaturas e pipeline (candidato e recrutamento)." },
                    new() { Name = "Companies", Description = "Endpoints para administração de cadastros corporativos" },
                    new() { Name = "Notifications", Description = "Alertas e comunicações (reservado)." },
                    new() { Name = "Admin", Description = "Superfície administrativa. Não aparece no documento EmpregaNet API (v1)." }
                };
            }
        }

        public static IApplicationBuilder UseSwaggerSetup(this IApplicationBuilder app)
        {

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{Constants.OpenApi.V1}/swagger.json", "EmpregaNet API");
                // c.SwaggerEndpoint($"/swagger/{Constants.OpenApi.Admin}/swagger.json", "Administração"); - documento separado para endpoints administrativos, se necessário.
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelsExpandDepth(-1);
            });

            return app;
        }
    }
}
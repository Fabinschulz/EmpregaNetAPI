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

                // Documento único por enquanto. Um documento "admin" separado exigiria
                // [ApiExplorerSettings(GroupName = ...)] nos controllers privilegiados, que
                // hoje nenhum declara. sem isso, o filtro de inclusão esvaziaria os dois.

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

                // Suporte a polimorfismo em schemas usando oneOf
                s.UseOneOfForPolymorphism();

                s.DocumentFilter<TagDescriptionsDocumentFilter>();

                // Registrado uma única vez: o filtro é global ao documento, e repeti-lo por
                // assembly de comentários (como estava) só o executava três vezes sem efeito extra.
                s.SchemaFilter<IgnoreEnumSchemaFilter>();

                // Comentários XML dos três assemblies que contribuem com tipos para o contrato.
                // O da própria API vem primeiro (nome resolvido em runtime); os demais são fixos.
                var xmlFileNames = new[]
                {
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml",
                    "EmpregaNet.Application.xml",
                    "EmpregaNet.Domain.xml"
                };

                foreach (var xmlFileName in xmlFileNames)
                {
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
                    if (!File.Exists(xmlPath))
                        continue;

                    s.IncludeXmlComments(xmlPath);
                    s.SchemaFilter<DescribeEnumMembers>(XDocument.Load(xmlPath));
                }
            });

            return services;
        }

        public class TagDescriptionsDocumentFilter : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                swaggerDoc.Tags = new HashSet<OpenApiTag>
                {
                    new() { Name = "Admin", Description = "Superfície administrativa." },
                    new() { Name = "Auth", Description = "Autenticação e credenciais: entrar, sair, registrar, renovar sessão e recuperar acesso." },
                    new() { Name = "Candidates", Description = "Listagem e detalhe de candidatos (equipe de recrutamento)." },
                    new() { Name = "Companies", Description = "Endpoints para administração de cadastros corporativos" },
                    new() { Name = "JobApplications", Description = "Candidaturas e pipeline (candidato e recrutamento)." },
                    new() { Name = "Jobs", Description = "Oportunidades de emprego (leitura pública; mutações com política de recrutamento)." },
                    new() { Name = "Notifications", Description = "Alertas e comunicações (reservado)." },
                    new() { Name = "Users", Description = "Dados da própria conta (/me): perfil, senha e encerramento." }
                };
            }
        }

        public static IApplicationBuilder UseSwaggerSetup(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
            if (!env.IsDevelopment() && !env.IsStaging())
            {
                return app;
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{Constants.OpenApi.V1}/swagger.json", "EmpregaNet API");
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelsExpandDepth(-1);

                // Ordena grupos e endpoints alfabeticamente na tela. Sem isto a ordem seguiria
                // a declaração no documento, o que não é confiável (é um conjunto) e deixaria
                // qualquer tag nova cair no fim da lista.
                c.ConfigObject.AdditionalItems["tagsSorter"] = "alpha";
                c.ConfigObject.AdditionalItems["operationsSorter"] = "alpha";
            });

            return app;
        }
    }
}
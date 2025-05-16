using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSwag.Generation.Processors.Security;

namespace EmpregaNet.Infra.Configurations
{
    public static class SwaggerConfig
    {
        public static WebApplicationBuilder AddSwaggerDoc(this WebApplicationBuilder builder)
        {
            builder.Services.AddOpenApiDocument(op =>
            {
                op.DocumentName = "v1";
                op.Title = "EmpregaNet API";
                op.Version = "v1";
                op.Description = "Uma API para cadastro de vagas de emprego";
                op.DocumentProcessors.Add(new SecurityDefinitionAppender(
                    JwtBearerDefaults.AuthenticationScheme,
                    new NSwag.OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Description = "Insira o token JWT no formato: Bearer {seu token}",
                        Type = NSwag.OpenApiSecuritySchemeType.Http,
                        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        BearerFormat = "JWT"
                    }
                ));
                op.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(JwtBearerDefaults.AuthenticationScheme));
            });

            return builder;
        }
    }
}
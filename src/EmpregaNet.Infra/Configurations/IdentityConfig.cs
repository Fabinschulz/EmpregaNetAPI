using System.Security.Claims;
using System.Text;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EmpregaNet.Infra.Configurations
{
    public static class IdentityConfig
    {

        public static WebApplicationBuilder AddIdentityConfiguration(this WebApplicationBuilder builder)
        {

            builder.AddJwtSupport()
                   .AddIdentityApiSupport();

            return builder;
        }

        public static WebApplicationBuilder AddIdentityApiSupport(this WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<User, Role>(options =>
                    {
                        // Configurações de senha
                        options.Password.RequireDigit = true;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireNonAlphanumeric = true;
                        options.Password.RequireUppercase = false;
                        options.Password.RequiredLength = 8;

                        // Configurações de Lockout
                        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                        options.Lockout.MaxFailedAccessAttempts = 5;
                        options.Lockout.AllowedForNewUsers = true;

                        // Configurações de usuário
                        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
                        options.User.RequireUniqueEmail = true;
                    })
                    .AddEntityFrameworkStores<PostgreSqlContext>()
                    .AddRoleManager<RoleManager<Role>>()
                    .AddSignInManager<SignInManager<User>>()
                    .AddUserManager<UserManager<User>>()
                    .AddApiEndpoints()
                    .AddDefaultTokenProviders(); // Adiciona suporte a tokens (confirmação de email, reset de senha, etc.)

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Administrador", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Admin");
                });
            });

            return builder;
        }


        private static WebApplicationBuilder AddJwtSupport(this WebApplicationBuilder builder)
        {
            var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
                throw new InvalidOperationException("JwtSettings ou SecretKey não configurado(s) no appsettings.json ou variáveis de ambiente.");

            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddJwtBearer((options =>
                                    {
                                        options.Events = new JwtBearerEvents
                                        {
                                            OnAuthenticationFailed = context =>
                                            {
                                                Console.WriteLine("TOKEN FALHOU: " + context.Exception.Message);
                                                return Task.CompletedTask;
                                            },

                                            OnForbidden = async context =>
                                            {
                                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                                context.Response.ContentType = "application/json";

                                                var domainError = new DomainError
                                                {
                                                    StatusCode = 403,
                                                    Code = DomainErrorEnum.MISSING_RESOURCE_PERMISSION,
                                                    Message = "Acesso negado. Você não possui permissão para esta operação.",
                                                    CorrelationId = context.HttpContext.Items["Correlation-ID"]?.ToString() ?? Guid.NewGuid().ToString(),
                                                    Details = null!
                                                };

                                                await context.Response.WriteAsJsonAsync(domainError);
                                            },

                                            OnChallenge = async context =>
                                            {
                                                context.HandleResponse();
                                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                                context.Response.ContentType = "application/json";

                                                var domainError = new DomainError
                                                {
                                                    StatusCode = 401,
                                                    Code = DomainErrorEnum.MISSING_RESOURCE_PERMISSION,
                                                    Message = "Token inválido ou não informado.",
                                                    CorrelationId = context.HttpContext.Items["Correlation-ID"]?.ToString() ?? Guid.NewGuid().ToString(),
                                                    Details = null!
                                                };

                                                await context.Response.WriteAsJsonAsync(domainError);
                                            }
                                        };

                                        options.RequireHttpsMetadata = true;
                                        options.SaveToken = true;
                                        options.TokenValidationParameters = new TokenValidationParameters
                                        {
                                            ValidateIssuerSigningKey = true,
                                            IssuerSigningKey = new SymmetricSecurityKey(key),
                                            ValidateIssuer = true,
                                            ValidateAudience = true,
                                            ValidateLifetime = true,
                                            ValidAudience = jwtSettings.Audience,
                                            ValidIssuer = jwtSettings.Issuer,
                                            NameClaimType = ClaimTypes.NameIdentifier
                                        };
                                    }));

            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.Configure<JwtSettings>(jwtSettingsSection);
            return builder;
        }
    }
}
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EmpregaNet.Infra.Configurations
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public int ExpirationHours { get; set; }
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }

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
            builder.Services.AddIdentity<User, IdentityRole<long>>(options =>
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
                        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                        options.User.RequireUniqueEmail = true;
                    })
                    .AddEntityFrameworkStores<PostgreSqlContext>()
                    .AddRoleManager<RoleManager<IdentityRole<long>>>()
                    .AddSignInManager<SignInManager<User>>()
                    .AddUserManager<UserManager<User>>()
                    .AddApiEndpoints()
                    .AddDefaultTokenProviders(); // Adiciona suporte a tokens (confirmação de email, reset de senha, etc.)

            // Configuração de cache e proteção de dados
            builder.Services.AddMemoryCache().AddDataProtection();

            return builder;
        }


        private static WebApplicationBuilder AddJwtSupport(this WebApplicationBuilder builder)
        {
            var jwtSettings = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<JwtSettings>>().Value;

            if (jwtSettings == null)
            {
                throw new InvalidOperationException("JwtSettings não configurado no appsettings.json ou variáveis de ambiente.");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddJwtBearer(options =>
                                    {
                                        options.SaveToken = true;
                                        options.TokenValidationParameters = new TokenValidationParameters
                                        {
                                            ValidateIssuerSigningKey = true,
                                            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                                            ValidateIssuer = true,
                                            ValidateAudience = true,
                                            ValidateLifetime = true,
                                            ValidAudience = jwtSettings.Audience,
                                            ValidIssuer = jwtSettings.Issuer
                                        };
                                    });

            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();

            return builder;
        }
    }
}
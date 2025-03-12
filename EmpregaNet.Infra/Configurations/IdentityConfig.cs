using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Configurations
{
    public static class IdentityConfig
    {
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>(options =>
                    {
                        // Configurações de senha
                        options.Password.RequireDigit = true;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireNonAlphanumeric = true;
                        options.Password.RequireUppercase = false;
                        options.Password.RequiredUniqueChars = 0;
                        options.Password.RequiredLength = 8;

                        // Configurações de Lockout
                        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                        options.Lockout.MaxFailedAccessAttempts = 5;
                        options.Lockout.AllowedForNewUsers = true;

                        // Configurações de usuário
                        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                        options.User.RequireUniqueEmail = false;
                    })
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders(); // Adiciona suporte a tokens (confirmação de email, reset de senha, etc.)

            // Configuração de cache e proteção de dados
            services.AddMemoryCache()
                    .AddDataProtection();

            return services;
        }
    }
}
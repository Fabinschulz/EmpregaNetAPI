
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Configurations
{
    public static class IdentityConfig
    {
        public static void AddIdentityConfiguration(this IServiceCollection services)
        {

            services.AddIdentityApiEndpoints<User>()
                    .AddEntityFrameworkStores<AppDbContext>();

            services.AddMemoryCache()
                    .AddDataProtection();

            services.AddIdentity<User, IdentityRole>(o =>
                    {
                        o.Password.RequireDigit = false;
                        o.Password.RequireLowercase = false;
                        o.Password.RequireNonAlphanumeric = false;
                        o.Password.RequireUppercase = false;
                        o.Password.RequiredUniqueChars = 0;
                        o.Password.RequiredLength = 8;
                    })
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();
        }
    }
}
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Services;
using EmpregaNet.Infra.Cache.MemoryService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Configurations;

public static class DIRegistrationServicesConfig
{
    public static void DIRegistrationServices(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<IEmailSender<User>, IdentityNoOpEmailSender>();
        services.AddTransient<IEmailSender<User>, EmailSender>();
        services.AddSingleton<IMemoryService, MemoryService>();
        services.AddScoped<CompanyService>();
    }

}

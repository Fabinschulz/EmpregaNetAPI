using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Domain.Services;
using EmpregaNet.Infra.Cache.MemoryService;
using EmpregaNet.Infra.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Configurations;

public static class DIRegistrationServicesConfig
{
    public static void DIRegistrationServices(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));        
        // services.AddTransient<IEmailSender<User>, IdentityNoOpEmailSender>();
        // services.AddTransient<IEmailSender<User>, EmailSender>();
        services.AddSingleton<IMemoryService, MemoryService>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<CompanyService>();
    }

}

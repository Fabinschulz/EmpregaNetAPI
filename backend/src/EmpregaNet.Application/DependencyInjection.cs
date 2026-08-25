using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FluentValidation;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Application.Jobs.UseCase;

public static class DependencyInjection
{
    public static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services)
    {
        var assemblies = Assembly.GetExecutingAssembly();
        services.AddValidatorsFromAssembly(assemblies);
        services.AddScoped<IJwtBuilder, JwtBuilder>();
        services.AddScoped<IJobEmployerAccess, JobEmployerAccess>();

        #region Dashboard
        services.AddScoped<IDashboardScopeAccess, DashboardScopeAccess>();
        services.AddScoped<IDashboardContextFactory, DashboardContextFactory>();
        services.AddScoped<IDashboardPeriodResolver, DashboardPeriodResolver>();
        #endregion

        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
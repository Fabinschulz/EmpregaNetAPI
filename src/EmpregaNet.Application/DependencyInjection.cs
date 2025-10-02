using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using EmpregaNet.Application.Interfaces;

public static class DependencyInjection
{
    public static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services)
    {
        var assemblies = Assembly.GetExecutingAssembly();
        services.AddValidatorsFromAssembly(assemblies);
        services.AddScoped<IJwtBuilder, JwtBuilder>();

        return services;
    }
}
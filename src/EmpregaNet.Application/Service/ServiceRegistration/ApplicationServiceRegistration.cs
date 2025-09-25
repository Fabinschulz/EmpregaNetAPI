using System.Reflection;
using EmpregaNet.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using EmpregaNet.Application.Service.Auth;

namespace EmpregaNet.Application.Service.ServiceRegistration;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection RegisterApplicationDependency(this IServiceCollection services)
    {
        services.AddScoped<IJwtBuilder, JwtBuilder>();

        var assemblies = Assembly.GetExecutingAssembly();
        services.AddValidatorsFromAssembly(assemblies);

        // Adiciona o comportamento de validação antes da execução de qualquer Handler.
        // Ele intercepta a requisição, executa as validações necessárias e, se falhar, evita que o Handler seja executado.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

        return services;
    }
}
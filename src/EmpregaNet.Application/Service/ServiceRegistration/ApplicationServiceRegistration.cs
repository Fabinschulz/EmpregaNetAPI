using System.Reflection;
using EmpregaNet.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using EmpregaNet.Infra.Configurations;
using Microsoft.Extensions.Configuration;
using FluentValidation;
using EmpregaNet.Domain.Components.Mediator.Interfaces;
using EmpregaNet.Application.Service.Auth;

namespace EmpregaNet.Application.Service.ServiceRegistration;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection RegisterApplicationDependency(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddScoped<IJwtBuilder, JwtBuilder>();

        var assemblies = Assembly.GetExecutingAssembly();
        services.AddValidatorsFromAssembly(assemblies);

        // Adiciona o comportamento de validação antes da execução de qualquer Handler.
        // Ele intercepta a requisição, executa as validações necessárias e, se falhar, evita que o Handler seja executado.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        // Adiciona o comportamento de medição de performance em cada requisição.
        // Se o tempo de execução ultrapassar um limite (por exemplo, 500ms), será logado um aviso.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

        return services;
    }
}
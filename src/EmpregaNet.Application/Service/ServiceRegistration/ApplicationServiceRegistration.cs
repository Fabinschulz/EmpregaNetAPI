using System.Reflection;
using EmpregaNet.Application.Common.Behaviors;
using Mapper.Interfaces;
using EmpregaNet.Infra.Utils;
using EmpregaNet.Domain.Components.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Mediator.Interfaces;
using EmpregaNet.Infra.Configurations;
using Microsoft.Extensions.Configuration;
using FluentValidation;

namespace EmpregaNet.Application.Service;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection RegisterApplicationDependency(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddScoped<IJwtBuilder, JwtBuilder>();

        var assemblies = Assembly.GetExecutingAssembly();

        var config = new MapperConfiguration();
        MappingProfileRunner.ApplyMappingsFromAssembly(assemblies, config);
        MappingProfileRunner.ApplyCustomMappingsFromAssembly(assemblies, config);
        config.AssertConfigurationIsValid();
        services.AddSingleton<IMapperConfigurationProvider>(config);
        services.AddSingleton<IMapper>(new MapperObj(config));
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
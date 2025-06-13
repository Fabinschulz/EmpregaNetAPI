using System.Reflection;
using EmpregaNet.Application.Common.Behaviors;
using Mapper.Interfaces;
using EmpregaNet.Infra.Utils;
using EmpregaNet.Domain.Components.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Mediator.Interfaces;

namespace EmpregaNet.Application.Service;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection RegisterApplicationDependency(this IServiceCollection services)
    {

        var assemblies = Assembly.GetExecutingAssembly();
        var config = new MapperConfiguration();

        // Aplica automaticamente todos os mapeamentos padrão encontrados na assembly
        MappingProfileRunner.ApplyMappingsFromAssembly(assemblies, config);

        // Aplica automaticamente todos os mapeamentos personalizados encontrados na assembly
        MappingProfileRunner.ApplyCustomMappingsFromAssembly(assemblies, config);

        // Garante que a configuração está válida, evitando mapeamentos incompletos ou incorretos
        config.AssertConfigurationIsValid();

        // Registra a configuração como singleton, garantindo que será compartilhada pela aplicação
        services.AddSingleton<IConfigurationProvider>(config);

        // Registra o Mapper como singleton, utilizando a configuração criada
        services.AddSingleton<IMapper>(new MapperObj(config));

        // Adiciona o comportamento de validação antes da execução de qualquer Handler.
        // Ele intercepta a requisição, executa as validações necessárias e, se falhar, evita que o Handler seja executado.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Adiciona o comportamento de medição de performance em cada requisição.
        // Se o tempo de execução ultrapassar um limite (por exemplo, 500ms), será logado um aviso.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

        return services;
    }
}
using System.Reflection;
using EmpregaNet.Application.Common.Behaviors;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Domain.Mapper.Interface;
using EmpregaNet.Infra.Utils;
using Mapper.Implementations;
using Mapper.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Application.Service;

public static class MapperInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
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

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
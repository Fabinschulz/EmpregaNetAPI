using System.Reflection;
using EmpregaNet.Domain.Mapper.Interface;
using EmpregaNet.Domain.Components.Mapper.Interfaces;

namespace EmpregaNet.Infra.Utils;

/// <summary>
/// Utilitário para aplicar automaticamente as configurações de mapeamento encontradas em uma <see cref="Assembly"/>.
/// Responsável por localizar e executar implementações de <see cref="IMapFrom{T}"/> e <see cref="ICustomMap"/>.
/// </summary>
public static class MappingProfileRunner
{
    /// <summary>
    /// Aplica todos os mapeamentos padrão (<see cref="IMapFrom{T}"/>) encontrados na assembly.
    /// </summary>
    /// <param name="assembly">Assembly que será escaneada.</param>
    /// <param name="config">Instância de configuração do mapeamento.</param>
    public static void ApplyMappingsFromAssembly(Assembly assembly, IMapperConfigurationExpression config)
    {
        // Localiza todos os tipos que implementam IMapFrom<>
        var types = assembly
            .GetExportedTypes()
            .Where(w =>
                w.GetInterfaces()
                 .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IMapFrom<>))
            )
            .ToList();

        foreach (var type in types)
        {
            // Cria uma instância do tipo encontrado
            var instance = Activator.CreateInstance(type);

            // Obtém o método Mapping: busca primeiro na própria classe, depois na interface
            var methodInfo = type.GetMethod("Mapping")
                ?? type.GetInterface("IMapFrom`1")!.GetMethod("Mapping");

            // Invoca o método, passando a configuração
            methodInfo?.Invoke(instance, new object[] { config });
        }
    }

    /// <summary>
    /// Aplica todas as configurações customizadas (<see cref="ICustomMap"/>) encontradas na assembly.
    /// </summary>
    /// <param name="assembly">Assembly que será escaneada.</param>
    /// <param name="config">Instância de configuração do mapeamento.</param>
    public static void ApplyCustomMappingsFromAssembly(Assembly assembly, IMapperConfigurationExpression config)
    {
        // Localiza todos os tipos que implementam ICustomMap
        var types = assembly
            .DefinedTypes
            .Where(w => w.ImplementedInterfaces.Contains(typeof(ICustomMap)));

        foreach (var type in types)
        {
            // Cria uma instância do tipo encontrado
            var instance = Activator.CreateInstance(type);

            // Obtém o método CustomMap
            var methodInfo = type.GetMethod("CustomMap");

            // Invoca o método, passando a configuração
            methodInfo?.Invoke(instance, new object[] { config });
        }
    }
}

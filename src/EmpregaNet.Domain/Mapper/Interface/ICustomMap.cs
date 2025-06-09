using EmpregaNet.Domain.Mapper.Interface;

namespace Mapper.Interfaces;

/// <summary>
/// Define um contrato para implementação de mapeamentos personalizados.
/// Classes que necessitam de lógica específica de mapeamento devem implementar esta interface.
/// </summary>
public interface ICustomMap
{
    /// <summary>
    /// Método que configura o mapeamento personalizado.
    /// </summary>
    /// <param name="configuration">Instância de configuração de mapeamento onde as regras personalizadas devem ser aplicadas.</param>
    void CustomMap(IMapperConfigurationExpression configuration);
}

namespace EmpregaNet.Domain.Mapper.Interface;

/// <summary>
/// Define um contrato para configuração automática de mapeamento de um tipo de origem para o tipo que implementa esta interface.
/// Utilizado para mapeamentos baseados em convenção.
/// </summary>
/// <typeparam name="T">Tipo de origem do mapeamento.</typeparam>
public interface IMapFrom<T>
{
    /// <summary>
    /// Configura o mapeamento padrão entre o tipo <typeparamref name="T"/> e o tipo que implementa esta interface.
    /// </summary>
    /// <param name="config">Instância de configuração de mapeamento.</param>
    void Mapping(IMapperConfigurationExpression config) => config.CreateMap(typeof(T), GetType());
}

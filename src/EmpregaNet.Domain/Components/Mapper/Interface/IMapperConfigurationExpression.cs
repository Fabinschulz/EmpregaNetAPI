namespace Mapper.Interfaces;

/// <summary>
/// Define um contrato para configuração de mapeamentos entre tipos.
/// Permite configurar mapeamentos simples ou com opções avançadas.
/// </summary>
public interface IMapperConfigurationExpression
{
    /// <summary>
    /// Configura um mapeamento padrão entre o tipo <typeparamref name="TSource"/> e <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    void CreateMap<TSource, TDestination>();

    /// <summary>
    /// Configura um mapeamento dinâmico entre os tipos fornecidos.
    /// </summary>
    /// <param name="source">Tipo de origem.</param>
    /// <param name="destination">Tipo de destino.</param>
    void CreateMap(System.Type source, System.Type destination);

    /// <summary>
    /// Configura um mapeamento entre os tipos <typeparamref name="TSource"/> e <typeparamref name="TDestination"/> com suporte a opções avançadas.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    /// <returns>Instância de <see cref="IMappingExpression{TSource, TDestination}"/> para configuração fluente.</returns>
    IMappingExpression<TSource, TDestination> CreateMapWithOptions<TSource, TDestination>();
}

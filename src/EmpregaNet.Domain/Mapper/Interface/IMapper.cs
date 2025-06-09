namespace EmpregaNet.Domain.Mapper.Interface;

public interface IMapper
{
    /// <summary>
    /// Mapeia uma instância do tipo <typeparamref name="TSource"/> para o tipo <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    /// <param name="source">Instância de origem a ser mapeada.</param>
    /// <returns>Instância do tipo de destino.</returns>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Mapeia uma coleção de instâncias do tipo <typeparamref name="TSource"/> para uma coleção de <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    /// <param name="sourceList">Coleção de instâncias de origem a serem mapeadas.</param>
    /// <returns>Coleção de instâncias do tipo de destino.</returns>
    IEnumerable<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> sourceList);
}
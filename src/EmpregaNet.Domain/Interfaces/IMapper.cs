namespace EmpregaNet.Domain.Interfaces;

public interface IMapper
{
    /// <summary>
    /// Mapeia uma instância de Source para uma nova instância de Destination
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Mapeia uma instância de Source para uma instância existente de Destination
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <remarks>
    /// Este método é útil quando você já tem uma instância de destino e deseja atualizar seus valores com os dados de origem.
    /// Ele não cria uma nova instância de destino, mas sim preenche os valores existentes com os dados do objeto de origem.
    /// </remarks>
    void Map<TSource, TDestination>(TSource source, TDestination destination);
}

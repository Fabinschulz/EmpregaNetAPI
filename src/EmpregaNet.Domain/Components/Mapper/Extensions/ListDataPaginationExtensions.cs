using EmpregaNet.Domain.Common;
using Mapper.Interfaces;

namespace EmpregaNet.Domain.Components.Mapper;

/// <summary>
/// Extensões para projeção de objetos em uma instância de <see cref="ListDataPagination{T}"/>.
/// </summary>
public static class ListDataPaginationExtensions
{
    /// <summary>
    /// Projeta os elementos da instância de <see cref="ListDataPagination{TSource}"/> para <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem dos elementos.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino dos elementos.</typeparam>
    /// <param name="source">Instância paginada contendo os dados de origem.</param>
    /// <param name="configuration">Configuração do mapeamento utilizada para realizar a projeção.</param>
    /// <returns>Nova instância de <see cref="ListDataPagination{TDestination}"/> com os dados projetados.</returns>
    public static ListDataPagination<TDestination> ProjectToListDataPagination<TSource, TDestination>(
        this ListDataPagination<TSource> source,
        IMapperConfigurationProvider configuration)
    {
        var projectedData = source.Data
            .AsQueryable()
            .ProjectToList<TSource, TDestination>(configuration);

        var pageSize = source.TotalItems / (source.TotalPages == 0 ? 1 : source.TotalPages);

        return new ListDataPagination<TDestination>(
            projectedData,
            source.TotalItems,
            source.Page,
            pageSize
        );
    }

    /// <summary>
    /// Projeta assíncronamente os elementos da instância de <see cref="ListDataPagination{TSource}"/> para <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem dos elementos.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino dos elementos.</typeparam>
    /// <param name="pagination">Instância paginada contendo os dados de origem.</param>
    /// <param name="configuration">Configuração do mapeamento utilizada para realizar a projeção.</param>
    /// <returns>
    /// Uma <see cref="Task"/> que representa a operação assíncrona, contendo como resultado uma instância de 
    /// <see cref="ListDataPagination{TDestination}"/> com os dados projetados.
    /// </returns>
    public static Task<ListDataPagination<TDestination>> ProjectToListDataPaginationAsync<TSource, TDestination>(
            this ListDataPagination<TSource> pagination,
            IMapperConfigurationProvider configuration)
    {
        var projectedData = pagination.Data
            .AsQueryable()
            .ProjectToList<TSource, TDestination>(configuration);

        var pageSize = pagination.TotalItems / (pagination.TotalPages == 0 ? 1 : pagination.TotalPages);

        return Task.FromResult(new ListDataPagination<TDestination>(
            projectedData,
            pagination.TotalItems,
            pagination.Page,
            pageSize));
    }
}

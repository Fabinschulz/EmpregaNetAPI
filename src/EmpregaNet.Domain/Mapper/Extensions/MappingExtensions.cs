using EmpregaNet.Domain.Common;
using Mapper.Interfaces;

namespace EmpregaNet.Mapper.Extensions;

/// <summary>
/// Extensões para facilitar a paginação e projeção de consultas <see cref="IQueryable{T}"/>.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Cria assincronamente uma lista paginada de <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TDestination">Tipo dos elementos.</typeparam>
    /// <param name="queryable">Fonte de dados.</param>
    /// <param name="pageNumber">Número da página.</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <returns>Uma <see cref="Task"/> contendo a lista paginada.</returns>
    public static Task<ListDataPagination<TDestination>> PaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable, int pageNumber, int pageSize)
        => ListDataPagination<TDestination>.CreateAsync(queryable, pageNumber, pageSize);

}

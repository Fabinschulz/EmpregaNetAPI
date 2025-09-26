using EmpregaNet.Domain.Common;
using Microsoft.EntityFrameworkCore;

public static class PaginationExtensions
{
    /// <summary>
    /// Cria assincronamente uma lista paginada de <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TDestination">Tipo dos elementos.</typeparam>
    /// <param name="source">Fonte de dados.</param>
    /// <param name="pageNumber">Número da página.</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <returns>Uma <see cref="Task"/> contendo a lista paginada.</returns>
    public static async Task<ListDataPagination<TDestination>> ToPaginatedListAsync<TDestination>(
        this IQueryable<TDestination> source,
        int pageNumber,
        int pageSize)
    {
        var totalItems = await source.CountAsync();

        var data = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Distinct()
            .ToListAsync();

        return new ListDataPagination<TDestination>(data, totalItems, pageNumber, pageSize);
    }
}
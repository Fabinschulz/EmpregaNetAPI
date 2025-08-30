using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Domain.Common;

public class ListDataPagination<T>
{
    public int Page { get; set; } = 0;
    public int TotalPages { get; set; } = 0;
    public int TotalItems { get; set; } = 0;
    public List<T> Data { get; set; } = new List<T>();

    public ListDataPagination(List<T> data, int totalItems, int page, int pageSize)
    {
        var totalPage = (int)Math.Ceiling(totalItems / (double)pageSize);
        Page = page;
        TotalPages = totalPage > 0 ? totalPage : 0;
        TotalItems = totalItems;
        Data = data;
    }

    /// <summary>
    ///  Cria uma instância paginada de forma assíncrona.
    /// Usage: await ListDataPagination<TDestination/>.CreateAsync(queryable, pageNumber, pageSize);
    /// </summary>
    /// <param name="source">Fonte de dados.</param>
    /// <param name="pageNumber">Número da página.</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <returns> Uma instância de <see cref="ListDataPagination{T}"/> com os dados paginados.</returns>
    public static async Task<ListDataPagination<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var totalItems = await source.CountAsync();
        var data = await source.Skip(pageNumber * pageSize).Take(pageSize).ToListAsync();
        return new ListDataPagination<T>(data, totalItems, pageNumber, pageSize);
    }
}

public static class PaginationExtensions
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
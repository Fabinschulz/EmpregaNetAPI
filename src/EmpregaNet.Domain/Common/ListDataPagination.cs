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
    /// Cria uma instância paginada de forma assíncrona.
    /// </summary>
    public static async Task<ListDataPagination<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var totalItems = await source.CountAsync();
        var data = await source.Skip(pageNumber * pageSize).Take(pageSize).ToListAsync();
        return new ListDataPagination<T>(data, totalItems, pageNumber, pageSize);
    }
}


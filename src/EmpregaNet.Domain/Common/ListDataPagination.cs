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
}
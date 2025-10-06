
using EmpregaNet.Domain.Common;

namespace EmpregaNet.Domain.Interfaces;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<ListDataPagination<T>> GetAllAsync(int Page, int Size, string? orderBy);
    Task<T?> GetByIdAsync(long id);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(long id);

}


using EmpregaNet.Domain.Common;

namespace EmpregaNet.Domain.Interfaces;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<ListDataPagination<T>> GetAllAsync(CancellationToken cancellationToken = default, int Page = 1, int Size = 10, string? orderBy = null);
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

}

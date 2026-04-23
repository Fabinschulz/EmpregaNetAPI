using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Repositories;

public class JobRepository : BaseRepository<Job>, IJobRepository
{
    public JobRepository(PostgreSqlContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByTitleAndCompanyIdAsync(string title, long companyId)
    {
        return await _context.Jobs.AnyAsync(j => j.Title == title && j.CompanyId == companyId);
    }

    public async Task<ListDataPagination<Job>> GetAllAsync(
        CancellationToken cancellationToken,
        int page,
        int size,
        string? orderBy,
        bool? isDeleted,
        bool? isActive)
    {
        var query = _context.Jobs.AsNoTracking();
        if (isDeleted.HasValue)
            query = query.Where(j => j.IsDeleted == isDeleted.Value);
        if (isActive.HasValue)
            query = query.Where(j => j.IsActive == isActive.Value);
        query = ApplyJobOrderBy(query, orderBy);
        return await query.ToPaginatedListAsync(page, size, cancellationToken);
    }

    private static IQueryable<Job> ApplyJobOrderBy(IQueryable<Job> query, string? orderBy)
    {
        return orderBy switch
        {
            "createdAt_ASC" => query.OrderBy(x => x.CreatedAt),
            "createdAt_DESC" => query.OrderByDescending(x => x.CreatedAt),
            "updatedAt_ASC" => query.OrderBy(x => x.UpdatedAt),
            "updatedAt_DESC" => query.OrderByDescending(x => x.UpdatedAt),
            "id_ASC" => query.OrderBy(x => x.Id),
            "id_DESC" => query.OrderByDescending(x => x.Id),
            _ => query.OrderByDescending(x => x.CreatedAt),
        };
    }
}

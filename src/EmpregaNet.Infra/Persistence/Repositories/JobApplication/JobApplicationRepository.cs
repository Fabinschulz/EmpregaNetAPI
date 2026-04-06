using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Repositories;

public class JobApplicationRepository : BaseRepository<JobApplication>, IJobApplicationRepository
{
    public JobApplicationRepository(PostgreSqlContext context) : base(context)
    {
    }

    public async Task<bool> ExistsAsync(long jobId, long userId, CancellationToken cancellationToken)
    {
        return await _context.JobApplications
            .AsNoTracking()
            .AnyAsync(a => a.JobId == jobId && a.UserId == userId && !a.IsDeleted, cancellationToken);
    }

    public async Task<ListDataPagination<JobApplication>> GetByJobIdAsync(
        long jobId,
        CancellationToken cancellationToken,
        int page,
        int size,
        ApplicationStatusEnum? status = null,
        string? orderBy = null)
    {
        var query = _context.JobApplications
            .AsNoTracking()
            .Where(a => a.JobId == jobId && !a.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = ApplyOrderBy(query, orderBy);
        }

        return await query.ToPaginatedListAsync(page, size, cancellationToken);
    }

    public async Task<ListDataPagination<JobApplication>> GetByUserIdAsync(
        long userId,
        CancellationToken cancellationToken,
        int page,
        int size,
        ApplicationStatusEnum? status = null,
        string? orderBy = null)
    {
        var query = _context.JobApplications
            .AsNoTracking()
            .Where(a => a.UserId == userId && !a.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = ApplyOrderBy(query, orderBy);
        }

        return await query.ToPaginatedListAsync(page, size, cancellationToken);
    }

    private static IQueryable<JobApplication> ApplyOrderBy(IQueryable<JobApplication> query, string orderBy)
    {
        return orderBy switch
        {
            "createdAt_ASC" => query.OrderBy(x => x.CreatedAt),
            "createdAt_DESC" => query.OrderByDescending(x => x.CreatedAt),
            "updatedAt_ASC" => query.OrderBy(x => x.UpdatedAt),
            "updatedAt_DESC" => query.OrderByDescending(x => x.UpdatedAt),
            "id_ASC" => query.OrderBy(x => x.Id),
            "id_DESC" => query.OrderByDescending(x => x.Id),
            "appliedAt_ASC" => query.OrderBy(x => x.AppliedAt),
            "appliedAt_DESC" => query.OrderByDescending(x => x.AppliedAt),
            _ => query.OrderByDescending(x => x.AppliedAt)
        };
    }
}

using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Interfaces;

public interface IJobApplicationRepository : IBaseRepository<JobApplication>
{
    Task<bool> ExistsAsync(long jobId, long userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<long>> GetAppliedJobIdsAsync(
        long userId,
        IReadOnlyCollection<long> jobIds,
        CancellationToken cancellationToken);

    Task<ListDataPagination<JobApplication>> GetByJobIdAsync(
        long jobId,
        CancellationToken cancellationToken,
        int page,
        int size,
        ApplicationStatusEnum? status = null,
        string? orderBy = null);

    Task<ListDataPagination<JobApplication>> GetByUserIdAsync(
        long userId,
        CancellationToken cancellationToken,
        int page,
        int size,
        ApplicationStatusEnum? status = null,
        string? orderBy = null);
}

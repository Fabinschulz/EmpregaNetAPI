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

    Task<JobApplicationProjection?> GetProjectionByIdAsync(long id, CancellationToken cancellationToken);

    Task<ListDataPagination<JobApplicationProjection>> GetAllWithCandidateAsync(
        CancellationToken cancellationToken,
        int page,
        int size,
        string? orderBy = null);

    Task<ListDataPagination<JobApplicationProjection>> GetByJobIdAsync(
        long jobId,
        CancellationToken cancellationToken,
        int page,
        int size,
        ApplicationStatusEnum? status = null,
        string? orderBy = null);

    Task<ListDataPagination<JobApplicationProjection>> GetByUserIdAsync(
        long userId,
        CancellationToken cancellationToken,
        int page,
        int size,
        ApplicationStatusEnum? status = null,
        string? orderBy = null);
}

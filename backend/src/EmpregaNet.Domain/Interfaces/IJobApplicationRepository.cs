using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Interfaces;

public interface IJobApplicationRepository : IBaseRepository<JobApplication>
{
    /// <summary>
    /// Já existe candidatura <b>activa</b> deste utilizador para esta vaga?
    /// </summary>
    Task<bool> ExistsActiveAsync(long jobId, long userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<long>> GetAppliedJobIdsAsync(
        long userId,
        IReadOnlyCollection<long> jobIds,
        CancellationToken cancellationToken);

    Task<JobApplicationProjection?> GetProjectionByIdAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Dados de notificação de uma candidatura (vaga, empresa e candidato) resolvidos numa consulta.
    /// </summary>
    Task<JobApplicationNotificationProjection?> GetNotificationProjectionAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Candidaturas da vaga cujo processo está em aberto (<see cref="JobApplication.OpenStatuses"/>),
    /// <b>rastreadas</b> para poderem ser alteradas.
    /// </summary>
    Task<IReadOnlyList<JobApplication>> GetOpenByJobIdAsync(long jobId, CancellationToken cancellationToken);

    Task<int> CountOpenByJobIdAsync(long jobId, CancellationToken cancellationToken);

    /// <summary>
    /// Quantas candidaturas activas o utilizador tem em cada status. Agregado no banco: a ficha do
    /// candidato precisa da contagem de candidaturas por status, mas não precisa de todas as candidaturas.
    /// </summary>
    Task<IReadOnlyDictionary<ApplicationStatusEnum, int>> GetStatusCountsByUserAsync(
        long userId,
        CancellationToken cancellationToken);

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

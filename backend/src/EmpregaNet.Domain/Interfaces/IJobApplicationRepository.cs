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

    /// <summary>
    /// Existe qualquer candidatura para esta vaga, independentemente do status? Usado para bloquear a
    /// exclusão da vaga: sem FK entre <c>Job</c> e <c>JobApplication</c>, apagar uma vaga com
    /// candidaturas (mesmo concluídas) deixaria histórico de candidatos apontando para um registro
    /// sumido.
    /// </summary>
    Task<bool> ExistsByJobIdAsync(long jobId, CancellationToken cancellationToken);

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

    /// <summary>
    /// Todas as candidaturas (recrutamento). <paramref name="companyId"/> restringe às vagas dessa
    /// empresa - Admin passa <c>null</c> e vê a plataforma inteira; Recruiter/Manager sempre passam
    /// a própria empresa (ver <c>IJobEmployerAccess.ResolveCompanyScopeAsync</c>).
    /// <paramref name="status"/> e <paramref name="search"/> (nome/e-mail do candidato ou título da vaga,
    /// case-insensitive) são aplicados antes da paginação: <c>TotalItems</c> reflete o filtro.
    /// </summary>
    Task<ListDataPagination<JobApplicationProjection>> GetAllWithCandidateAsync(
        CancellationToken cancellationToken,
        int page,
        int size,
        string? orderBy = null,
        long? companyId = null,
        ApplicationStatusEnum? status = null,
        string? search = null);

    /// <summary>
    /// Candidaturas de uma vaga. <paramref name="search"/> segue a mesma regra de
    /// <see cref="GetAllWithCandidateAsync"/> e também é aplicado antes da paginação.
    /// </summary>
    Task<ListDataPagination<JobApplicationProjection>> GetByJobIdAsync(
        long jobId,
        CancellationToken cancellationToken,
        int page,
        int size,
        ApplicationStatusEnum? status = null,
        string? orderBy = null,
        string? search = null);

    Task<ListDataPagination<JobApplicationProjection>> GetByUserIdAsync(
        long userId,
        CancellationToken cancellationToken,
        int page,
        int size,
        ApplicationStatusEnum? status = null,
        string? orderBy = null);
}

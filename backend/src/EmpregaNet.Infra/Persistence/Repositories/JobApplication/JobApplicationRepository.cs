using System.Linq.Expressions;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Repositories;

public class JobApplicationRepository : BaseRepository<JobApplication>, IJobApplicationRepository
{
    /// <summary>
    /// O que conta como candidatura <b>activa</b> para efeito de duplicata e de "já candidatado".
    /// </summary>
    /// <remarks>
    /// Predicado único, e não a mesma condição escrita em dois <c>Where</c>: quem decide se pode
    /// candidatar-se (<see cref="ExistsActiveAsync"/>) e quem decide se o feed mostra o botão
    /// (<see cref="GetAppliedJobIdsAsync"/>) têm de responder sempre o mesmo. Divergirem significa a
    /// API aceitar uma candidatura que a tela bloqueia, ou o contrário.
    /// </remarks>
    private static readonly Expression<Func<JobApplication, bool>> IsActiveApplication =
        a => !a.IsDeleted && a.Status != ApplicationStatusEnum.CanceledByCandidate;

    public JobApplicationRepository(PostgreSqlContext context) : base(context)
    {
    }

    public async Task<bool> ExistsActiveAsync(long jobId, long userId, CancellationToken cancellationToken)
    {
        return await _context.JobApplications
            .AsNoTracking()
            .Where(IsActiveApplication)
            .AnyAsync(a => a.JobId == jobId && a.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Vagas, entre as consultadas, em que o utilizador tem candidatura activa, é o que faz o feed
    /// mostrar "Já candidatado" em vez do botão de candidatura.
    /// </summary>
    public async Task<IReadOnlyList<long>> GetAppliedJobIdsAsync(
        long userId,
        IReadOnlyCollection<long> jobIds,
        CancellationToken cancellationToken)
    {
        if (jobIds.Count == 0)
        {
            return Array.Empty<long>();
        }

        var ids = jobIds.Distinct().ToArray();

        return await _context.JobApplications
            .AsNoTracking()
            .Where(IsActiveApplication)
            .Where(a => a.UserId == userId && ids.Contains(a.JobId))
            .Select(a => a.JobId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<JobApplicationProjection?> GetProjectionByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await ProjectWithCandidate(_context.JobApplications.AsNoTracking().Where(a => a.Id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<ApplicationStatusEnum, int>> GetStatusCountsByUserAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var counts = await _context.JobApplications
            .AsNoTracking()
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<ListDataPagination<JobApplicationProjection>> GetAllWithCandidateAsync(
        CancellationToken cancellationToken,
        int page,
        int size,
        string? orderBy = null)
    {
        var query = _context.JobApplications
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        return await ProjectWithCandidate(ApplyOrderBy(query, orderBy))
            .ToPaginatedListAsync(page, size, cancellationToken);
    }

    public async Task<ListDataPagination<JobApplicationProjection>> GetByJobIdAsync(
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

        return await ProjectWithCandidate(ApplyOrderBy(query, orderBy))
            .ToPaginatedListAsync(page, size, cancellationToken);
    }

    public async Task<ListDataPagination<JobApplicationProjection>> GetByUserIdAsync(
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

        return await ProjectWithCandidate(ApplyOrderBy(query, orderBy))
            .ToPaginatedListAsync(page, size, cancellationToken);
    }

    /// <summary>
    /// Resolve o candidato junto da candidatura, em uma consulta só.
    /// </summary>
    /// <remarks>
    /// É um LEFT JOIN (<c>DefaultIfEmpty</c>) e não um INNER: o usuário é excluído logicamente, mas
    /// se uma linha órfã existir por qualquer motivo, um INNER a faria sumir da listagem em
    /// silêncio - a candidatura desapareceria da tela sem erro nenhum. Com LEFT, a candidatura
    /// aparece e o nome vem vazio, que é um problema visível.
    ///
    /// O candidato excluído continua sendo devolvido, com <c>IsDeleted</c> marcado: o histórico do
    /// processo seletivo precisa dele, e cabe à tela decidir como sinalizar.
    /// </remarks>
    private IQueryable<JobApplicationProjection> ProjectWithCandidate(IQueryable<JobApplication> applications)
    {
        return from application in applications
               join user in _context.Users.AsNoTracking() on application.UserId equals user.Id into candidates
               from candidate in candidates.DefaultIfEmpty()
               select new JobApplicationProjection(
                   application.Id,
                   application.JobId,
                   new JobApplicationCandidate(
                       application.UserId,
                       candidate != null ? (candidate.UserName ?? string.Empty) : string.Empty,
                       candidate != null ? (candidate.Email ?? string.Empty) : string.Empty,
                       candidate != null && candidate.IsDeleted),
                   application.Status,
                   application.AppliedAt,
                   application.CreatedAt,
                   application.UpdatedAt,
                   application.DeletedAt,
                   application.IsDeleted);
    }

    private static IQueryable<JobApplication> ApplyOrderBy(IQueryable<JobApplication> query, string? orderBy)
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

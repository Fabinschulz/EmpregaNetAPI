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

    public async Task<bool> ExistsByJobIdAsync(long jobId, CancellationToken cancellationToken)
    {
        return await _context.JobApplications
            .AsNoTracking()
            .AnyAsync(a => a.JobId == jobId && !a.IsDeleted, cancellationToken);
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
        var joined = JoinCandidateAndJob(_context.JobApplications.AsNoTracking().Where(a => a.Id == id));

        return await ProjectWithCandidate(joined).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<JobApplicationNotificationProjection?> GetNotificationProjectionAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var query =
            from application in _context.JobApplications.AsNoTracking()
            where application.Id == id
            join job in _context.Jobs.AsNoTracking() on application.JobId equals job.Id into jobs
            from job in jobs.DefaultIfEmpty()
            join company in _context.Companies.AsNoTracking() on job.CompanyId equals company.Id into companies
            from company in companies.DefaultIfEmpty()
            join user in _context.Users.AsNoTracking() on application.UserId equals user.Id into candidates
            from candidate in candidates.DefaultIfEmpty()
            select new JobApplicationNotificationProjection(
                application.Id,
                application.JobId,
                job != null ? job.Title : string.Empty,
                company != null ? company.CompanyName : string.Empty,
                application.UserId,
                candidate != null ? (candidate.UserName ?? string.Empty) : string.Empty,
                candidate != null ? (candidate.Email ?? string.Empty) : string.Empty);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobApplication>> GetOpenByJobIdAsync(long jobId, CancellationToken cancellationToken)
    {
        return await _context.JobApplications
            .Where(a => a.JobId == jobId
                        && !a.IsDeleted
                        && JobApplication.OpenStatuses.Contains(a.Status))
            .OrderBy(a => a.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountOpenByJobIdAsync(long jobId, CancellationToken cancellationToken)
    {
        return await _context.JobApplications
            .AsNoTracking()
            .CountAsync(
                a => a.JobId == jobId
                     && !a.IsDeleted
                     && JobApplication.OpenStatuses.Contains(a.Status),
                cancellationToken);
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
        string? orderBy = null,
        long? companyId = null,
        ApplicationStatusEnum? status = null,
        string? search = null)
    {
        var query = _context.JobApplications
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (companyId.HasValue)
        {
            query =
                from application in query
                join job in _context.Jobs.AsNoTracking() on application.JobId equals job.Id
                where job.CompanyId == companyId.Value
                select application;
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await ListWithCandidateAsync(query, search, orderBy, page, size, cancellationToken);
    }

    public async Task<ListDataPagination<JobApplicationProjection>> GetByJobIdAsync(
        long jobId,
        CancellationToken cancellationToken,
        int page,
        int size,
        ApplicationStatusEnum? status = null,
        string? orderBy = null,
        string? search = null)
    {
        var query = _context.JobApplications
            .AsNoTracking()
            .Where(a => a.JobId == jobId && !a.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await ListWithCandidateAsync(query, search, orderBy, page, size, cancellationToken);
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

        return await ListWithCandidateAsync(query, search: null, orderBy, page, size, cancellationToken);
    }

    /// <summary>
    /// Cadeia única das listagens: junta vaga e candidato, filtra pela busca, ordena, projeta e
    /// pagina, nessa ordem. A busca entra <b>antes</b> da paginação; filtrar depois paginaria sobre o
    /// conjunto errado e o <c>TotalItems</c> não refletiria o filtro.
    /// </summary>
    private Task<ListDataPagination<JobApplicationProjection>> ListWithCandidateAsync(
        IQueryable<JobApplication> applications,
        string? search,
        string? orderBy,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var joined = ApplySearch(JoinCandidateAndJob(applications), search);

        return ProjectWithCandidate(ApplyOrderBy(joined, orderBy))
            .ToPaginatedListAsync(page, size, cancellationToken);
    }

    /// <summary>
    /// Junta a vaga e o candidato à candidatura, em uma consulta só, expondo as três linhas para que
    /// filtros sobre vaga/candidato possam ser compostos antes da projeção.
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
    private IQueryable<ApplicationWithJobAndCandidate> JoinCandidateAndJob(IQueryable<JobApplication> applications)
    {
        return from application in applications
               join job in _context.Jobs.AsNoTracking() on application.JobId equals job.Id into jobs
               from job in jobs.DefaultIfEmpty()
               join user in _context.Users.AsNoTracking() on application.UserId equals user.Id into candidates
               from candidate in candidates.DefaultIfEmpty()
               select new ApplicationWithJobAndCandidate
               {
                   Application = application,
                   Job = job,
                   Candidate = candidate
               };
    }

    /// <summary>
    /// Busca textual por título da vaga, nome de usuário ou e-mail do candidato (case-insensitive).
    /// Termo vazio não filtra.
    /// </summary>
    private static IQueryable<ApplicationWithJobAndCandidate> ApplySearch(
        IQueryable<ApplicationWithJobAndCandidate> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim().ToLower();

        return query.Where(x =>
            (x.Job != null && x.Job.Title.ToLower().Contains(term)) ||
            (x.Candidate != null && x.Candidate.UserName != null && x.Candidate.UserName.ToLower().Contains(term)) ||
            (x.Candidate != null && x.Candidate.Email != null && x.Candidate.Email.ToLower().Contains(term)));
    }

    private static IQueryable<JobApplicationProjection> ProjectWithCandidate(
        IQueryable<ApplicationWithJobAndCandidate> joined)
    {
        return joined.Select(x => new JobApplicationProjection(
            x.Application.Id,
            x.Application.JobId,
            x.Job != null ? x.Job.Title : string.Empty,
            x.Job != null && !x.Job.IsDeleted && x.Job.IsActive,
            new JobApplicationCandidate(
                x.Application.UserId,
                x.Candidate != null ? (x.Candidate.UserName ?? string.Empty) : string.Empty,
                x.Candidate != null ? (x.Candidate.Email ?? string.Empty) : string.Empty,
                x.Candidate != null && x.Candidate.IsDeleted),
            x.Application.Status,
            x.Application.AppliedAt,
            x.Application.CreatedAt,
            x.Application.UpdatedAt,
            x.Application.DeletedAt,
            x.Application.IsDeleted));
    }

    /// <summary>
    /// Ordena sobre as linhas já juntadas (antes da projeção): os campos de ordenação são todos da
    /// candidatura, e ordenar sobre o record projetado por construtor não é traduzível.
    /// </summary>
    private static IQueryable<ApplicationWithJobAndCandidate> ApplyOrderBy(
        IQueryable<ApplicationWithJobAndCandidate> query,
        string? orderBy)
    {
        return orderBy switch
        {
            "createdAt_ASC" => query.OrderBy(x => x.Application.CreatedAt),
            "createdAt_DESC" => query.OrderByDescending(x => x.Application.CreatedAt),
            "updatedAt_ASC" => query.OrderBy(x => x.Application.UpdatedAt),
            "updatedAt_DESC" => query.OrderByDescending(x => x.Application.UpdatedAt),
            "id_ASC" => query.OrderBy(x => x.Application.Id),
            "id_DESC" => query.OrderByDescending(x => x.Application.Id),
            "appliedAt_ASC" => query.OrderBy(x => x.Application.AppliedAt),
            "appliedAt_DESC" => query.OrderByDescending(x => x.Application.AppliedAt),
            _ => query.OrderByDescending(x => x.Application.AppliedAt)
        };
    }

    /// <summary>
    /// Candidatura + vaga + candidato carregados pelo LEFT JOIN das listagens.
    /// </summary>
    /// <remarks>
    /// Classe de leitura e não <c>ValueTuple</c>: árvore de expressão não aceita literal de tupla, e
    /// a composição posterior (<c>Where</c>/<c>OrderBy</c>) sobre membros de uma classe com
    /// inicializador é a forma que o provider já traduz aqui (mesmo padrão de <c>JobWithCompany</c>
    /// em <c>JobRepository</c>).
    /// </remarks>
    private sealed class ApplicationWithJobAndCandidate
    {
        public required JobApplication Application { get; init; }
        public Job? Job { get; init; }
        public User? Candidate { get; init; }
    }
}

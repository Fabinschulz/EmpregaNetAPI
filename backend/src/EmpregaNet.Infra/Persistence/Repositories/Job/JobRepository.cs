using System.Linq.Expressions;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace EmpregaNet.Infra.Persistence.Repositories;

public class JobRepository : BaseRepository<Job>, IJobRepository
{
    private const char LikeEscapeCharacter = '\\';

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
        bool? isActive,
        string? search = null)
    {
        var query = _context.Jobs.AsNoTracking();
        if (isDeleted.HasValue)
            query = query.Where(j => j.IsDeleted == isDeleted.Value);
        if (isActive.HasValue)
            query = query.Where(j => j.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(term) ||
                j.Description.ToLower().Contains(term));
        }
        query = ApplyJobOrderBy(query, orderBy);
        return await query.ToPaginatedListAsync(page, size, cancellationToken);
    }

    public async Task<ListDataPagination<JobFeedProjection>> GetFeedAsync(
        JobFeedFilter filter,
        CancellationToken cancellationToken)
    {
        var query = BuildFeedQuery(filter);

        var totalItems = await query.CountAsync(cancellationToken);

        var data = await ApplyFeedOrder(query, filter)
            .Skip((filter.Page - 1) * filter.Size)
            .Take(filter.Size)
            .Select(x => new JobFeedProjection(
                x.Job.Id,
                x.Job.Title,
                x.Job.Summary,
                new JobFeedCompany(x.Company.Id, x.Company.CompanyName),
                new JobFeedLocation(x.Job.Location.City, x.Job.Location.State, x.Job.Location.Country),
                new JobFeedSalary(x.Job.SalaryMin, x.Job.SalaryMax, x.Job.SalaryDisclosed),
                x.Job.JobType,
                x.Job.WorkModel,
                x.Job.WorkShift,
                x.Job.ExperienceLevel,
                x.Job.Area,
                x.Job.IsPcdFriendly,
                x.Job.Requirements.ToList(),
                x.Job.Benefits.ToList(),
                x.Job.PublishedAt,
                _context.JobApplications.Count(a => a.JobId == x.Job.Id && !a.IsDeleted),
                x.Job.IsActive))
            .ToListAsync(cancellationToken);

        return new ListDataPagination<JobFeedProjection>(data, totalItems, filter.Page, filter.Size);
    }

    private IQueryable<JobWithCompany> BuildFeedQuery(JobFeedFilter filter)
    {
        var query =
            from job in _context.Jobs.AsNoTracking()
            join company in _context.Companies.AsNoTracking() on job.CompanyId equals company.Id
            where !job.IsDeleted && job.IsActive && !company.IsDeleted
            select new JobWithCompany { Job = job, Company = company };

        if (filter.HasSearch)
        {
            var term = filter.Search!.Trim();
            var namePattern = $"%{EscapeLikePattern(term)}%";

            query = query.Where(x =>
                EF.Property<NpgsqlTsVector>(x.Job, JobSearchVector.PropertyName)
                  .Matches(EF.Functions.WebSearchToTsQuery(JobSearchVector.SearchConfiguration, term))
                || EF.Functions.ILike(x.Company.CompanyName, namePattern, LikeEscapeCharacter.ToString()));
        }

        query = WhereSelected(query, filter.Cities, v => x => v.Contains(x.Job.Location.City));
        query = WhereSelected(query, filter.States, v => x => v.Contains(x.Job.Location.State));
        query = WhereSelected(query, filter.WorkModels, v => x => v.Contains(x.Job.WorkModel));
        query = WhereSelected(query, filter.WorkShifts, v => x => v.Contains(x.Job.WorkShift));
        query = WhereSelected(query, filter.JobTypes, v => x => v.Contains(x.Job.JobType));
        query = WhereSelected(query, filter.ExperienceLevels, v => x => v.Contains(x.Job.ExperienceLevel));
        query = WhereSelected(query, filter.Areas, v => x => v.Contains(x.Job.Area));
        query = WhereSelected(query, filter.CompanyIds, v => x => v.Contains(x.Job.CompanyId));

        // Sobreposição de arrays resolvida pelo índice GIN, sem join.
        query = WhereSelected(query, filter.Requirements, v => x => x.Job.Requirements.Any(r => v.Contains(r)));
        query = WhereSelected(query, filter.Benefits, v => x => x.Job.Benefits.Any(b => v.Contains(b)));

        if (filter.OnlyPcdFriendly)
        {
            query = query.Where(x => x.Job.IsPcdFriendly);
        }

        if (filter.PublishedAfter.HasValue)
        {
            var publishedAfter = filter.PublishedAfter.Value;
            query = query.Where(x => x.Job.PublishedAt >= publishedAfter);
        }

        if (filter.HasSalaryBound)
        {
            // Faixa "a combinar" sai do resultado: devolvê-la num filtro por salário seria
            // apresentar como compatível uma vaga cujo salário ninguém conhece.
            query = query.Where(x => x.Job.SalaryDisclosed);

            // Interseção de intervalos. O coalesce cobre a vaga que anuncia valor único
            // (só SalaryMin) sem a excluir indevidamente.
            if (filter.SalaryMin.HasValue)
            {
                var min = filter.SalaryMin.Value;
                query = query.Where(x => (x.Job.SalaryMax ?? x.Job.SalaryMin) >= min);
            }

            if (filter.SalaryMax.HasValue)
            {
                var max = filter.SalaryMax.Value;
                query = query.Where(x => (x.Job.SalaryMin ?? x.Job.SalaryMax) <= max);
            }
        }

        return query;
    }

    /// <summary>
    /// Ordenação do feed. O desempate por <c>Id</c> decrescente é obrigatório: sem uma chave
    /// única no ORDER BY, duas páginas consecutivas podem repetir ou saltar linhas empatadas.
    /// </summary>
    private static IQueryable<JobWithCompany> ApplyFeedOrder(IQueryable<JobWithCompany> query, JobFeedFilter filter)
    {
        var sort = filter.Sort == JobFeedSortEnum.Relevance && !filter.HasSearch
            ? JobFeedSortEnum.Recent
            : filter.Sort;

        switch (sort)
        {
            case JobFeedSortEnum.Salary:
                return query
                    .OrderByDescending(x => x.Job.SalaryMax ?? x.Job.SalaryMin)
                    .ThenByDescending(x => x.Job.SalaryMin)
                    .ThenByDescending(x => x.Job.Id);

            case JobFeedSortEnum.Relevance:
                var term = filter.Search!.Trim();
                return query
                    .OrderByDescending(x =>
                        EF.Property<NpgsqlTsVector>(x.Job, JobSearchVector.PropertyName)
                          .Rank(EF.Functions.WebSearchToTsQuery(JobSearchVector.SearchConfiguration, term)))
                    .ThenByDescending(x => x.Job.PublishedAt)
                    .ThenByDescending(x => x.Job.Id);

            case JobFeedSortEnum.Company:
                return query
                    .OrderBy(x => x.Company.CompanyName)
                    .ThenByDescending(x => x.Job.PublishedAt)
                    .ThenByDescending(x => x.Job.Id);

            case JobFeedSortEnum.Location:
                return query
                    .OrderBy(x => x.Job.Location.State)
                    .ThenBy(x => x.Job.Location.City)
                    .ThenByDescending(x => x.Job.PublishedAt)
                    .ThenByDescending(x => x.Job.Id);

            case JobFeedSortEnum.Recent:
            default:
                return query
                    .OrderByDescending(x => x.Job.PublishedAt)
                    .ThenByDescending(x => x.Job.Id);
        }
    }

    /// <summary>
    /// Neutraliza os coringas do <c>LIKE</c> no termo do utilizador.
    /// </summary>
    private static string EscapeLikePattern(string term) => term
        .Replace("\\", "\\\\")
        .Replace("%", "\\%")
        .Replace("_", "\\_");

    /// <summary>
    /// Aplica um filtro de seleção múltipla. Coleção vazia ou nula não filtra: "nenhuma opção
    /// marcada" significa "todas".
    /// </summary>
    /// <remarks>
    /// O predicado recebe os valores já materializados em array, forma que o provider traduz para
    /// um parâmetro único (<c>= ANY(@p)</c>) em vez de uma lista literal de <c>IN</c>.
    /// </remarks>
    private static IQueryable<JobWithCompany> WhereSelected<TValue>(
        IQueryable<JobWithCompany> query,
        IReadOnlyCollection<TValue>? selected,
        Func<TValue[], Expression<Func<JobWithCompany, bool>>> predicate)
        => selected is { Count: > 0 }
            ? query.Where(predicate(selected.ToArray()))
            : query;

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

    /// <summary>Par vaga + empresa carregado pelo join do feed.</summary>
    private sealed class JobWithCompany
    {
        public required Job Job { get; init; }
        public required Company Company { get; init; }
    }
}

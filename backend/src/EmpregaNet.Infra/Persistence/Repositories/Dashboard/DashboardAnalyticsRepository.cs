using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Repositories.Dashboard;

/// <summary>
/// Leituras agregadas do dashboard.
/// </summary>
public sealed class DashboardAnalyticsRepository : IDashboardAnalyticsRepository
{
    private readonly PostgreSqlContext _context;

    public DashboardAnalyticsRepository(PostgreSqlContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Contagens

    public async Task<DashboardComparedCounters> GetComparedCountersAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var current = filter.Range;
        var previous = current.Previous();

        var jobCounters = await Jobs(filter)
            .Where(j => j.PublishedAt >= previous.FromUtc && j.PublishedAt < current.ToUtcExclusive)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                CreatedCurrent = g.Count(j => j.PublishedAt >= current.FromUtc && j.PublishedAt < current.ToUtcExclusive),
                CreatedPrevious = g.Count(j => j.PublishedAt >= previous.FromUtc && j.PublishedAt < previous.ToUtcExclusive)
            })
            .ToSingleRowAsync(cancellationToken);

        var applicationCounters = await ApplicationsWithJob(filter)
            .Where(x =>
                x.Application.AppliedAt >= previous.FromUtc && x.Application.AppliedAt < current.ToUtcExclusive)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Current = g.Count(x =>
                    x.Application.AppliedAt >= current.FromUtc && x.Application.AppliedAt < current.ToUtcExclusive),
                Previous = g.Count(x =>
                    x.Application.AppliedAt >= previous.FromUtc && x.Application.AppliedAt < previous.ToUtcExclusive)
            })
            .ToSingleRowAsync(cancellationToken);

        var candidateCounters = await FirstApplicationPerCandidate(filter)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Current = g.Count(x => x.FirstAt >= current.FromUtc && x.FirstAt < current.ToUtcExclusive),
                Previous = g.Count(x => x.FirstAt >= previous.FromUtc && x.FirstAt < previous.ToUtcExclusive)
            })
            .ToSingleRowAsync(cancellationToken);

        return new DashboardComparedCounters(
            Current: new DashboardPeriodCounters(
                NewJobs: jobCounters?.CreatedCurrent ?? 0,
                NewApplications: applicationCounters?.Current ?? 0,
                NewCandidates: candidateCounters?.Current ?? 0),
            Previous: new DashboardPeriodCounters(
                NewJobs: jobCounters?.CreatedPrevious ?? 0,
                NewApplications: applicationCounters?.Previous ?? 0,
                NewCandidates: candidateCounters?.Previous ?? 0));
    }

    public async Task<DashboardTotals> GetTotalsAsync(DashboardFilter filter, CancellationToken cancellationToken)
    {
        var activeJobs = await JobsExistingAt(filter).CountAsync(j => j.IsActive, cancellationToken);

        var companiesWithActiveJobs = await JobsExistingAt(filter)
            .Where(j => j.IsActive)
            .Select(j => j.CompanyId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new DashboardTotals(
            ActiveJobs: activeJobs,
            CompaniesWithActiveJobs: companiesWithActiveJobs);
    }

    #endregion

    #region Séries temporais

    public Task<IReadOnlyList<DashboardDailyPoint>> GetJobsDailySeriesAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var range = filter.Range;

        return DailySeriesAsync(
            Jobs(filter)
                .Where(j => j.PublishedAt >= range.FromUtc && j.PublishedAt < range.ToUtcExclusive)
                .Select(j => j.PublishedAt),
            filter.LocalOffset,
            cancellationToken);
    }

    public Task<IReadOnlyList<DashboardDailyPoint>> GetApplicationsDailySeriesAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
        => DailySeriesAsync(
            ApplicationsInRange(filter).Select(a => a.AppliedAt),
            filter.LocalOffset,
            cancellationToken);

    public Task<IReadOnlyList<DashboardDailyPoint>> GetNewCandidatesDailySeriesAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var range = filter.Range;

        return DailySeriesAsync(
            FirstApplicationPerCandidate(filter)
                .Where(x => x.FirstAt >= range.FromUtc && x.FirstAt < range.ToUtcExclusive)
                .Select(x => x.FirstAt),
            filter.LocalOffset,
            cancellationToken);
    }

    #endregion

    #region Distribuições

    public async Task<IReadOnlyList<DashboardStatusComparison>> GetApplicationsByStatusAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var current = filter.Range;
        var previous = current.Previous();

        var rows = await ApplicationsWithJob(WithoutStatusFilter(filter))
            .Where(x => x.Application.AppliedAt >= previous.FromUtc && x.Application.AppliedAt < current.ToUtcExclusive)
            .GroupBy(x => x.Application.Status)
            .Select(g => new
            {
                Status = g.Key,
                Current = g.Count(x =>
                    x.Application.AppliedAt >= current.FromUtc && x.Application.AppliedAt < current.ToUtcExclusive),
                Previous = g.Count(x =>
                    x.Application.AppliedAt >= previous.FromUtc && x.Application.AppliedAt < previous.ToUtcExclusive)
            })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new DashboardStatusComparison(r.Status, r.Current, r.Previous))];
    }

    public async Task<IReadOnlyList<DashboardEnumCount<JobAreaEnum>>> GetJobsByAreaAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var range = filter.Range;

        var rows = await Jobs(filter)
            .Where(j => j.PublishedAt >= range.FromUtc && j.PublishedAt < range.ToUtcExclusive)
            .GroupBy(j => j.Area)
            .Select(g => new { Area = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new DashboardEnumCount<JobAreaEnum>(r.Area, r.Count))];
    }

    public async Task<IReadOnlyList<DashboardEnumCount<JobAreaEnum>>> GetApplicationsByAreaAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var rows = await ApplicationsWithJobInRange(filter)
            .GroupBy(x => x.Job.Area)
            .Select(g => new { Area = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new DashboardEnumCount<JobAreaEnum>(r.Area, r.Count))];
    }

    #endregion

    #region Performance de vagas

    public async Task<IReadOnlyList<DashboardJobPerformance>> GetJobPerformanceAsync(
        DashboardFilter filter,
        DashboardJobRankingEnum ranking,
        int limit,
        bool onlyActive,
        CancellationToken cancellationToken)
    {
        var range = filter.Range;
        var applications = ApplicationsOnly(filter);

        var jobs = JobsExistingAt(filter);
        if (onlyActive)
        {
            jobs = jobs.Where(j => j.IsActive);
        }

        var projected =
            from job in jobs
            join company in _context.Companies.AsNoTracking() on job.CompanyId equals company.Id
            select new
            {
                JobId = job.Id,
                job.Title,
                CompanyId = company.Id,
                company.CompanyName,
                City = job.Location.City,
                State = job.Location.State,
                job.Area,
                job.IsActive,
                job.PublishedAt,
                ApplicationsInRange = applications.Count(a =>
                    a.JobId == job.Id &&
                    a.AppliedAt >= range.FromUtc &&
                    a.AppliedAt < range.ToUtcExclusive),
                ApplicationsTotal = applications.Count(a =>
                    a.JobId == job.Id && a.AppliedAt < range.ToUtcExclusive),
                LastApplicationAt = applications
                    .Where(a => a.JobId == job.Id && a.AppliedAt < range.ToUtcExclusive)
                    .Max(a => (DateTimeOffset?)a.AppliedAt)
            };


        var ordered = ranking switch
        {
            DashboardJobRankingEnum.FewestApplications => projected
                .OrderBy(x => x.ApplicationsInRange)
                .ThenBy(x => x.PublishedAt)
                .ThenBy(x => x.JobId),
            DashboardJobRankingEnum.MostRecent => projected
                .OrderByDescending(x => x.PublishedAt)
                .ThenByDescending(x => x.JobId),
            _ => projected
                .OrderByDescending(x => x.ApplicationsInRange)
                .ThenByDescending(x => x.PublishedAt)
                .ThenByDescending(x => x.JobId)
        };

        var rows = await ordered.Take(limit).ToListAsync(cancellationToken);

        return [.. rows.Select(row => new DashboardJobPerformance(
            row.JobId,
            row.Title,
            row.CompanyId,
            row.CompanyName,
            row.City,
            row.State,
            row.Area,
            row.IsActive,
            row.PublishedAt,
            row.ApplicationsInRange,
            row.ApplicationsTotal,
            row.LastApplicationAt))];
    }

    public async Task<(double Average, int JobCount)> GetAverageApplicationsPerJobAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var jobCount = await JobsExistingAt(filter).CountAsync(cancellationToken);
        if (jobCount == 0)
        {
            return (0d, 0);
        }

        var applicationCount = await ApplicationsUpTo(filter).CountAsync(cancellationToken);

        return ((double)applicationCount / jobCount, jobCount);
    }

    public async Task<DashboardJobHealth> GetJobHealthAsync(
        DashboardFilter filter,
        int staleDays,
        CancellationToken cancellationToken)
    {
        var range = filter.Range;
        var cutoff = range.ToUtcExclusive.AddDays(-staleDays);
        var applications = ApplicationsOnly(filter);
        var jobs = JobsExistingAt(filter);

        var activeJobs = await jobs.CountAsync(j => j.IsActive, cancellationToken);

        var withoutApplications = await jobs
            .Where(j => j.IsActive)
            .Where(j => !applications.Any(a => a.JobId == j.Id && a.AppliedAt < range.ToUtcExclusive))
            .CountAsync(cancellationToken);

        var staleJobs = await jobs
            .Where(j => j.IsActive && j.PublishedAt < cutoff)
            .Where(j => !applications.Any(a => a.JobId == j.Id && a.AppliedAt >= cutoff))
            .CountAsync(cancellationToken);

        var closedInPeriod = await Jobs(filter)
            .Where(j => !j.IsActive && j.UpdatedAt != null)
            .Where(j => j.UpdatedAt >= range.FromUtc && j.UpdatedAt < range.ToUtcExclusive)
            .CountAsync(cancellationToken);

        return new DashboardJobHealth(activeJobs, withoutApplications, staleJobs, closedInPeriod);
    }

    #endregion

    #region Blocos reutilizados

    /// <summary>
    /// Vagas visíveis ao escopo, já com os recortes de UF e área.
    /// </summary>
    private IQueryable<Job> Jobs(DashboardFilter filter)
    {
        var query = _context.Jobs.AsNoTracking().Where(j => !j.IsDeleted);

        if (filter.Scope.CompanyId is { } companyId)
        {
            query = query.Where(j => j.CompanyId == companyId);
        }

        if (filter.HasStates)
        {
            var states = filter.States!.ToArray();
            query = query.Where(j => states.Contains(j.Location.State));
        }

        if (filter.HasAreas)
        {
            var areas = filter.Areas!.ToArray();
            query = query.Where(j => areas.Contains(j.Area));
        }

        return query;
    }

    /// <summary>Vagas que já existiam no fim da janela - base de todo número acumulado.</summary>
    private IQueryable<Job> JobsExistingAt(DashboardFilter filter)
        => Jobs(filter).Where(j => j.PublishedAt < filter.Range.ToUtcExclusive);

    /// <summary>
    /// Candidaturas não excluídas, com o recorte por status. Sem o join da vaga para uso em
    /// subconsulta já correlacionada por <c>JobId</c> a uma vaga do escopo.
    /// </summary>
    private IQueryable<JobApplication> ApplicationsOnly(DashboardFilter filter)
    {
        var query = _context.JobApplications.AsNoTracking().Where(a => !a.IsDeleted);

        if (filter.ApplicationStatus is { } status)
        {
            query = query.Where(a => a.Status == status);
        }

        return query;
    }

    /// <summary>Candidatura emparelhada com a vaga, respeitando escopo e recortes.</summary>
    private IQueryable<ApplicationWithJob> ApplicationsWithJob(DashboardFilter filter)
        => from application in ApplicationsOnly(filter)
           join job in Jobs(filter) on application.JobId equals job.Id
           select new ApplicationWithJob { Application = application, Job = job };

    private IQueryable<ApplicationWithJob> ApplicationsWithJobInRange(DashboardFilter filter)
    {
        var range = filter.Range;
        return ApplicationsWithJob(filter)
            .Where(x => x.Application.AppliedAt >= range.FromUtc && x.Application.AppliedAt < range.ToUtcExclusive);
    }

    private IQueryable<JobApplication> ApplicationsInRange(DashboardFilter filter)
        => ApplicationsWithJobInRange(filter).Select(x => x.Application);

    /// <summary>Candidaturas acumuladas até o fim da janela.</summary>
    private IQueryable<JobApplication> ApplicationsUpTo(DashboardFilter filter)
    {
        var cutoff = filter.Range.ToUtcExclusive;
        return ApplicationsWithJob(filter)
            .Where(x => x.Application.AppliedAt < cutoff)
            .Select(x => x.Application);
    }

    /// <summary>
    /// Primeira candidatura de cada utilizador dentro do escopo.
    /// </summary>
    private IQueryable<CandidateEntry> FirstApplicationPerCandidate(DashboardFilter filter)
        => ApplicationsWithJob(filter)
            .GroupBy(x => x.Application.UserId)
            .Select(g => new CandidateEntry
            {
                UserId = g.Key,
                FirstAt = g.Min(x => x.Application.AppliedAt)
            });

    /// <summary>
    /// Agrupa instantes por dia <b>local</b> e devolve os baldes em ordem cronológica.
    /// </summary>
    private static async Task<IReadOnlyList<DashboardDailyPoint>> DailySeriesAsync(
        IQueryable<DateTimeOffset> instants,
        TimeSpan offset,
        CancellationToken cancellationToken)
    {
        var rows = await instants
            .GroupBy(instant => new
            {
                Year = (instant.UtcDateTime + offset).Year,
                Month = (instant.UtcDateTime + offset).Month,
                Day = (instant.UtcDateTime + offset).Day
            })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return [.. rows
            .Select(r => new DashboardDailyPoint(new DateOnly(r.Year, r.Month, r.Day), r.Count))
            .OrderBy(point => point.Day)];
    }

    private static DashboardFilter WithoutStatusFilter(DashboardFilter filter)
        => filter with { ApplicationStatus = null };

    private sealed class ApplicationWithJob
    {
        public required JobApplication Application { get; init; }
        public required Job Job { get; init; }
    }

    private sealed class CandidateEntry
    {
        public required long UserId { get; init; }
        public required DateTimeOffset FirstAt { get; init; }
    }

    #endregion
}

/// <summary>Auxiliar de consulta usado apenas pelas leituras do dashboard.</summary>
internal static class DashboardQueryExtensions
{    public static async Task<TResult?> ToSingleRowAsync<TResult>(
        this IQueryable<TResult> query,
        CancellationToken cancellationToken)
        where TResult : class
    {
        var rows = await query.ToListAsync(cancellationToken);
        return rows.Count > 0 ? rows[0] : null;
    }
}

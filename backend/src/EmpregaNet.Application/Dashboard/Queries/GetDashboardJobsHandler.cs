using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Application.Dashboard.ViewModel;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Dashboard.Queries;

/// <summary>Ranking de desempenho das vagas.</summary>
/// <param name="Filter">Recortes do cabeçalho.</param>
/// <param name="Ranking">Critério: mais ou menos candidaturas.</param>
/// <param name="Limit">Linhas devolvidas.</param>
/// <param name="OnlyActive">Restringe a vagas abertas.</param>
public sealed record GetDashboardJobsQuery(
    DashboardFilterInput Filter,
    DashboardJobRankingEnum Ranking = DashboardJobRankingEnum.MostApplications,
    int Limit = 8,
    bool OnlyActive = true) : IRequest<DashboardJobsViewModel>;

public sealed class GetDashboardJobsHandler : IRequestHandler<GetDashboardJobsQuery, DashboardJobsViewModel>
{
    /// <summary>Desvio mínimo sobre a média para a vaga sair da faixa "na média".</summary>
    private const decimal PerformanceBandPercent = 10m;

    private readonly IDashboardAnalyticsRepository _repository;
    private readonly IDashboardContextFactory _contextFactory;
    private readonly ILogger<GetDashboardJobsHandler> _logger;

    public GetDashboardJobsHandler(
        IDashboardAnalyticsRepository repository,
        IDashboardContextFactory contextFactory,
        ILogger<GetDashboardJobsHandler> logger)
    {
        _repository = repository;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<DashboardJobsViewModel> Handle(
        GetDashboardJobsQuery request,
        CancellationToken cancellationToken)
    {
        var context = await _contextFactory.CreateAsync(request.Filter, cancellationToken);

        _logger.LogInformation(
            "Dashboard: performance de vagas ({Ranking}, limite {Limit}, apenas ativas: {OnlyActive})",
            request.Ranking, request.Limit, request.OnlyActive);

        var items = await _repository.GetJobPerformanceAsync(
            context.Filter, request.Ranking, request.Limit, request.OnlyActive, cancellationToken);

        var (average, jobsInAverage) = await _repository.GetAverageApplicationsPerJobAsync(
            context.Filter, cancellationToken);

        var averageRounded = Math.Round((decimal)average, 1, MidpointRounding.AwayFromZero);

        return new DashboardJobsViewModel
        {
            Meta = _contextFactory.CreateMeta(context, request.Filter, DashboardDomainGaps.Jobs),
            Ranking = request.Ranking.ToString(),
            RankingLabel = RankingLabel(request.Ranking),
            OnlyActive = request.OnlyActive,
            AverageApplicationsPerJob = averageRounded,
            JobsInAverage = jobsInAverage,
            Items = [.. items.Select(item => BuildItem(item, context, average))]
        };
    }

    private static string RankingLabel(DashboardJobRankingEnum ranking) => ranking switch
    {
        DashboardJobRankingEnum.FewestApplications => "Menos candidaturas",
        DashboardJobRankingEnum.MostRecent => "Mais recentes",
        _ => "Mais candidaturas"
    };

    private static DashboardJobPerformanceViewModel BuildItem(
        DashboardJobPerformance job,
        DashboardQueryContext context,
        double average)
    {
        var reference = context.Filter.Range.ToUtcExclusive;

        var deviation = average > 0
            ? Math.Round((job.TotalApplications - (decimal)average) * 100m / (decimal)average, 0, MidpointRounding.AwayFromZero)
            : (decimal?)null;

        return new DashboardJobPerformanceViewModel
        {
            Id = job.JobId,
            Title = job.Title,
            CompanyId = job.CompanyId,
            CompanyName = job.CompanyName,
            City = job.City,
            State = job.State.ToString(),
            Area = job.Area.ToString(),
            AreaLabel = job.Area.ToDescription(),
            IsActive = job.IsActive,
            StatusLabel = job.IsActive ? "Ativa" : "Encerrada",
            PublishedAt = BrasiliaTime.Format(job.PublishedAt, "dd/MM/yyyy"),
            DaysActive = WholeDaysBetween(job.PublishedAt, reference),
            Applications = job.Applications,
            TotalApplications = job.TotalApplications,
            LastApplicationAt = job.LastApplicationAt is { } last
                ? BrasiliaTime.Format(last, "dd/MM/yyyy")
                : null,
            DaysSinceLastApplication = job.LastApplicationAt is { } lastAt
                ? WholeDaysBetween(lastAt, reference)
                : null,
            PerformanceVsAverage = deviation,
            Performance = ClassifyPerformance(deviation)
        };
    }

    /// <summary>
    /// Classifica o desempenho em faixas.
    /// </summary>
    private static string ClassifyPerformance(decimal? deviation) => deviation switch
    {
        null => "none",
        var value when value >= PerformanceBandPercent => "above",
        var value when value <= -PerformanceBandPercent => "below",
        _ => "average"
    };

    private static int WholeDaysBetween(DateTimeOffset from, DateTimeOffset to)
    {
        var days = (int)(to - from).TotalDays;
        return days < 0 ? 0 : days;
    }
}

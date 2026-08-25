using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Application.Dashboard.ViewModel;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Dashboard.Queries;

public sealed record GetDashboardInsightsQuery(DashboardFilterInput Filter) : IRequest<DashboardInsightsViewModel>;

public sealed class GetDashboardInsightsHandler : IRequestHandler<GetDashboardInsightsQuery, DashboardInsightsViewModel>
{
    /// <summary>
    /// Dias sem candidatura a partir dos quais a vaga ativa é tratada como estagnada.
    /// </summary>
    public const int StaleJobDays = 30;

    private readonly IDashboardAnalyticsRepository _repository;
    private readonly IDashboardContextFactory _contextFactory;
    private readonly ILogger<GetDashboardInsightsHandler> _logger;

    public GetDashboardInsightsHandler(
        IDashboardAnalyticsRepository repository,
        IDashboardContextFactory contextFactory,
        ILogger<GetDashboardInsightsHandler> logger)
    {
        _repository = repository;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<DashboardInsightsViewModel> Handle(
        GetDashboardInsightsQuery request,
        CancellationToken cancellationToken)
    {
        var context = await _contextFactory.CreateAsync(request.Filter, cancellationToken);

        _logger.LogInformation("Dashboard: insights ({From} a {To})",
            context.Period.FromLocal, context.Period.ToLocalInclusive);

        var health = await _repository.GetJobHealthAsync(context.Filter, StaleJobDays, cancellationToken);
        var counters = await _repository.GetComparedCountersAsync(context.Filter, cancellationToken);
        var applicationsByArea = await _repository.GetApplicationsByAreaAsync(context.Filter, cancellationToken);
        var topJobs = await _repository.GetJobPerformanceAsync(
            context.Filter, DashboardJobRankingEnum.MostApplications, limit: 1, onlyActive: false, cancellationToken);
        var (average, _) = await _repository.GetAverageApplicationsPerJobAsync(context.Filter, cancellationToken);

        var topArea = applicationsByArea
            .Where(item => item.Value != Domain.Enums.JobAreaEnum.NaoSelecionado && item.Count > 0)
            .OrderByDescending(item => item.Count)
            .FirstOrDefault();

        var topJob = topJobs.FirstOrDefault();

        var input = new DashboardInsightsInput(
            ActiveJobs: health.ActiveJobs,
            StaleJobs: health.StaleJobs,
            StaleDays: StaleJobDays,
            JobsWithoutApplications: health.JobsWithoutApplications,
            Applications: counters.Current.NewApplications,
            PreviousApplications: counters.Previous.NewApplications,
            NewCandidates: counters.Current.NewCandidates,
            PreviousNewCandidates: counters.Previous.NewCandidates,
            TopApplicationArea: topArea is null ? null : topArea.Value.ToDescription(),
            TopApplicationAreaCount: topArea?.Count ?? 0,
            CategorizedApplications: applicationsByArea.Sum(item => item.Count),
            TopJobTitle: topJob?.Title,
            TopJobApplications: topJob?.TotalApplications ?? 0,
            AverageApplicationsPerJob: average);

        return new DashboardInsightsViewModel
        {
            Meta = _contextFactory.CreateMeta(context, request.Filter, DashboardDomainGaps.Overview),
            Items = DashboardInsightsBuilder.Build(input),
            StaleDays = StaleJobDays
        };
    }
}

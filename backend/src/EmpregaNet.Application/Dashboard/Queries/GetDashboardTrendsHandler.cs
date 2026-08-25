using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Application.Dashboard.ViewModel;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Dashboard.Queries;

/// <summary>Séries temporais do período.</summary>
/// <param name="Filter">Recortes do cabeçalho.</param>
/// <param name="Granularity">Granularidade pedida; nulo deixa o servidor escolher pelo tamanho da janela.</param>
public sealed record GetDashboardTrendsQuery(
    DashboardFilterInput Filter,
    DashboardGranularityEnum? Granularity = null) : IRequest<DashboardTrendsViewModel>;

public sealed class GetDashboardTrendsHandler : IRequestHandler<GetDashboardTrendsQuery, DashboardTrendsViewModel>
{
    private readonly IDashboardAnalyticsRepository _repository;
    private readonly IDashboardContextFactory _contextFactory;
    private readonly IDashboardPeriodResolver _periodResolver;
    private readonly ILogger<GetDashboardTrendsHandler> _logger;

    public GetDashboardTrendsHandler(
        IDashboardAnalyticsRepository repository,
        IDashboardContextFactory contextFactory,
        IDashboardPeriodResolver periodResolver,
        ILogger<GetDashboardTrendsHandler> logger)
    {
        _repository = repository;
        _contextFactory = contextFactory;
        _periodResolver = periodResolver;
        _logger = logger;
    }

    public async Task<DashboardTrendsViewModel> Handle(
        GetDashboardTrendsQuery request,
        CancellationToken cancellationToken)
    {
        var context = await _contextFactory.CreateAsync(request.Filter, cancellationToken);
        var granularity = _periodResolver.ResolveGranularity(context.Period, request.Granularity);

        _logger.LogInformation(
            "Dashboard: séries temporais ({Days} dias, granularidade {Granularity})",
            context.Period.DayCount,
            granularity);

        var jobs = await _repository.GetJobsDailySeriesAsync(context.Filter, cancellationToken);
        var applications = await _repository.GetApplicationsDailySeriesAsync(context.Filter, cancellationToken);
        var candidates = await _repository.GetNewCandidatesDailySeriesAsync(context.Filter, cancellationToken);
        
        var series = new List<DashboardSeriesViewModel>
        {
            DashboardSeriesBuilder.Build("applications", "Candidaturas", applications, context.Period, granularity),
            DashboardSeriesBuilder.Build("candidates", "Novos candidatos", candidates, context.Period, granularity),
            DashboardSeriesBuilder.Build("jobs", "Vagas publicadas", jobs, context.Period, granularity)
        };

        return new DashboardTrendsViewModel
        {
            Meta = _contextFactory.CreateMeta(context, request.Filter, DashboardDomainGaps.Trends),
            Granularity = granularity.ToString(),
            GranularityLabel = DashboardSeriesBuilder.GranularityLabel(granularity),
            Series = series
        };
    }
}

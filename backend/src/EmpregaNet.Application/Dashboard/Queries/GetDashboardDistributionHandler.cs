using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Application.Dashboard.ViewModel;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Dashboard.Queries;

public sealed record GetDashboardDistributionQuery(DashboardFilterInput Filter)
    : IRequest<DashboardDistributionViewModel>;

/// <summary>
/// Distribuições que o painel executivo mostra: status das candidaturas e concentração por área.
/// </summary>
public sealed class GetDashboardDistributionHandler
    : IRequestHandler<GetDashboardDistributionQuery, DashboardDistributionViewModel>
{
    private const int TopCategories = 8;

    private readonly IDashboardAnalyticsRepository _repository;
    private readonly IDashboardContextFactory _contextFactory;
    private readonly ILogger<GetDashboardDistributionHandler> _logger;

    public GetDashboardDistributionHandler(
        IDashboardAnalyticsRepository repository,
        IDashboardContextFactory contextFactory,
        ILogger<GetDashboardDistributionHandler> logger)
    {
        _repository = repository;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<DashboardDistributionViewModel> Handle(
        GetDashboardDistributionQuery request,
        CancellationToken cancellationToken)
    {
        var context = await _contextFactory.CreateAsync(request.Filter, cancellationToken);

        _logger.LogInformation("Dashboard: distribuições ({From} a {To})",
            context.Period.FromLocal, context.Period.ToLocalInclusive);

        var byStatus = await _repository.GetApplicationsByStatusAsync(context.Filter, cancellationToken);
        var applicationsByArea = await _repository.GetApplicationsByAreaAsync(context.Filter, cancellationToken);
        var jobsByArea = await _repository.GetJobsByAreaAsync(context.Filter, cancellationToken);

        return new DashboardDistributionViewModel
        {
            Meta = _contextFactory.CreateMeta(context, request.Filter, DashboardDomainGaps.Distribution),

            ApplicationsByStatus = DashboardBreakdownFactory.Create(
                [.. byStatus.Select(item => new DashboardEnumCount<ApplicationStatusEnum>(item.Status, item.Current))],
                neutral: ApplicationStatusEnum.NaoSelecionado),

            ApplicationsByArea = DashboardBreakdownFactory.Create(
                applicationsByArea,
                neutral: JobAreaEnum.NaoSelecionado,
                topN: TopCategories),

            JobsByArea = DashboardBreakdownFactory.Create(
                jobsByArea,
                neutral: JobAreaEnum.NaoSelecionado,
                topN: TopCategories)
        };
    }
}

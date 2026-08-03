using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Jobs.Queries;

/// <summary>
/// Consulta do feed público de vagas. Query dedicada em vez de sobrecarregar
/// <see cref="GetAllQuery{TResponse}"/> porque o feed é anónimo e cacheado publicamente.
/// </summary>
public sealed record GetJobsFeedQuery(
    int Page = 1,
    int Size = 20,
    string? Search = null,
    IReadOnlyCollection<string>? Cities = null,
    IReadOnlyCollection<UF>? States = null,
    IReadOnlyCollection<WorkModelEnum>? WorkModels = null,
    IReadOnlyCollection<WorkShiftEnum>? WorkShifts = null,
    IReadOnlyCollection<JobTypeEnum>? JobTypes = null,
    IReadOnlyCollection<ExperienceLevelEnum>? ExperienceLevels = null,
    IReadOnlyCollection<JobAreaEnum>? Areas = null,
    IReadOnlyCollection<string>? Requirements = null,
    IReadOnlyCollection<string>? Benefits = null,
    IReadOnlyCollection<long>? CompanyIds = null,
    decimal? SalaryMin = null,
    decimal? SalaryMax = null,
    bool OnlyPcdFriendly = false,
    JobPublishedWindowEnum PublishedWithin = JobPublishedWindowEnum.Any,
    JobFeedSortEnum Sort = JobFeedSortEnum.Recent
) : IRequest<ListDataPagination<JobFeedItemViewModel>>, IPaginatedQuery
{
    public string? OrderBy => null;
}

public sealed class GetJobsFeedHandler
    : IRequestHandler<GetJobsFeedQuery, ListDataPagination<JobFeedItemViewModel>>
{
    private readonly IJobRepository _repository;
    private readonly ILogger<GetJobsFeedHandler> _logger;
    private readonly TimeProvider _timeProvider;

    public GetJobsFeedHandler(
        IJobRepository repository,
        ILogger<GetJobsFeedHandler> logger,
        TimeProvider timeProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<ListDataPagination<JobFeedItemViewModel>> Handle(
        GetJobsFeedQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Consultando feed de vagas (Página: {Page}, Tamanho: {Size}, Ordem: {Sort}, Busca: {HasSearch})",
            request.Page, request.Size, request.Sort, !string.IsNullOrWhiteSpace(request.Search));

        try
        {
            var filter = new JobFeedFilter(
                Search: string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
                Cities: request.Cities,
                States: request.States,
                WorkModels: request.WorkModels,
                WorkShifts: request.WorkShifts,
                JobTypes: request.JobTypes,
                ExperienceLevels: request.ExperienceLevels,
                Areas: request.Areas,
                Requirements: request.Requirements,
                Benefits: request.Benefits,
                CompanyIds: request.CompanyIds,
                SalaryMin: request.SalaryMin,
                SalaryMax: request.SalaryMax,
                OnlyPcdFriendly: request.OnlyPcdFriendly,
                PublishedAfter: ResolvePublishedAfter(request.PublishedWithin),
                Sort: request.Sort,
                Page: request.Page,
                Size: request.Size);

            var result = await _repository.GetFeedAsync(filter, cancellationToken);
            var items = result.Data.Select(job => job.ToFeedItem()).ToList();

            _logger.LogInformation("Feed de vagas: {Count} resultados na página, {Total} no total.",
                items.Count, result.TotalItems);

            return new ListDataPagination<JobFeedItemViewModel>(
                items, result.TotalItems, request.Page, request.Size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao consultar o feed de vagas. Query: {@Query}", request);
            throw;
        }
    }

    private DateTimeOffset? ResolvePublishedAfter(JobPublishedWindowEnum window)
    {
        var nowUtc = _timeProvider.GetUtcNow();

        return window switch
        {
            JobPublishedWindowEnum.Today => StartOfTodayInBrasilia(nowUtc),
            JobPublishedWindowEnum.Last24Hours => nowUtc.AddHours(-24),
            JobPublishedWindowEnum.Last3Days => nowUtc.AddDays(-3),
            JobPublishedWindowEnum.Last7Days => nowUtc.AddDays(-7),
            JobPublishedWindowEnum.Last15Days => nowUtc.AddDays(-15),
            JobPublishedWindowEnum.Last30Days => nowUtc.AddDays(-30),
            _ => null
        };
    }

    /// <summary>
    /// "Hoje" é o dia do utilizador, não o dia UTC. Às 22h de Brasília já é o dia seguinte em UTC -
    /// usar UTC aqui esvaziaria o filtro todas as noites.
    /// </summary>
    private static DateTimeOffset StartOfTodayInBrasilia(DateTimeOffset nowUtc)
    {
        var timeZone = BrasiliaTime.GetBrasiliaTimeZone();
        var local = TimeZoneInfo.ConvertTime(nowUtc, timeZone);
        var startOfDay = new DateTimeOffset(local.Year, local.Month, local.Day, 0, 0, 0, local.Offset);

        return startOfDay.ToUniversalTime();
    }
}

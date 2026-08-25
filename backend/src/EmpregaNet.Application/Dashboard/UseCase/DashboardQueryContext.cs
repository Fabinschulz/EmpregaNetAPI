using EmpregaNet.Application.Dashboard.ViewModel;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Dashboard.UseCase;

public sealed record DashboardQueryContext(
    DashboardFilter Filter,
    DashboardResolvedPeriod Period,
    DashboardResolvedScope Scope)
{
    public DashboardFilter PreviousFilter => Filter.For(Filter.Range.Previous());
}

public interface IDashboardContextFactory
{
    Task<DashboardQueryContext> CreateAsync(DashboardFilterInput input, CancellationToken cancellationToken);
    DashboardMetaViewModel CreateMeta(
        DashboardQueryContext context,
        DashboardFilterInput input,
        IReadOnlyList<DashboardUnavailableMetricViewModel> unavailable);
}

/// <inheritdoc cref="IDashboardContextFactory"/>
public sealed class DashboardContextFactory : IDashboardContextFactory
{
    private const string DateFormat = "dd/MM/yyyy";

    private readonly IDashboardScopeAccess _scopeAccess;
    private readonly IDashboardPeriodResolver _periodResolver;
    private readonly TimeProvider _timeProvider;

    public DashboardContextFactory(
        IDashboardScopeAccess scopeAccess,
        IDashboardPeriodResolver periodResolver,
        TimeProvider timeProvider)
    {
        _scopeAccess = scopeAccess;
        _periodResolver = periodResolver;
        _timeProvider = timeProvider;
    }

    public async Task<DashboardQueryContext> CreateAsync(
        DashboardFilterInput input,
        CancellationToken cancellationToken)
    {
        var scope = await _scopeAccess.ResolveAsync(input.CompanyId, cancellationToken);
        var period = _periodResolver.Resolve(input.Period, input.From, input.To);

        var filter = new DashboardFilter(
            Scope: scope.Scope,
            Range: period.Range,
            LocalOffset: period.LocalOffset,
            States: NormalizeStates(input.States),
            Areas: NormalizeAreas(input.Areas),
            ApplicationStatus: NormalizeStatus(input.ApplicationStatus));

        return new DashboardQueryContext(filter, period, scope);
    }

    public DashboardMetaViewModel CreateMeta(
        DashboardQueryContext context,
        DashboardFilterInput input,
        IReadOnlyList<DashboardUnavailableMetricViewModel> unavailable)
    {
        var period = context.Period;
        var previous = period.Range.Previous();
        var timeZone = BrasiliaTime.GetBrasiliaTimeZone();

        return new DashboardMetaViewModel
        {
            Period = input.Period.ToString(),
            PeriodLabel = input.Period.ToDescription(),
            From = period.FromLocal.ToString(DateFormat),
            To = period.ToLocalInclusive.ToString(DateFormat),
            FromUtc = period.Range.FromUtc.ToString("O"),
            ToUtcExclusive = period.Range.ToUtcExclusive.ToString("O"),
            Days = period.DayCount,
            PreviousFrom = LocalDate(previous.FromUtc, timeZone),
            PreviousTo = LocalDate(previous.ToUtcExclusive.AddDays(-1), timeZone),
            GeneratedAt = BrasiliaTime.Format(_timeProvider.GetUtcNow()),
            Scope = BuildScope(context.Scope),
            AppliedFilters = BuildAppliedFilters(context.Filter),
            Unavailable = unavailable
        };
    }

    private static DashboardScopeViewModel BuildScope(DashboardResolvedScope scope)
        => scope.Scope.IsPlatformWide
            ? new DashboardScopeViewModel("platform", null, null)
            : new DashboardScopeViewModel("company", scope.Scope.CompanyId, scope.CompanyName);

    private static IReadOnlyList<string> BuildAppliedFilters(DashboardFilter filter)
    {
        var filters = new List<string>();

        if (filter.HasStates)
        {
            filters.Add($"UF: {string.Join(", ", filter.States!.Select(state => state.ToString()))}");
        }

        if (filter.HasAreas)
        {
            filters.Add($"Área: {string.Join(", ", filter.Areas!.Select(area => area.ToDescription()))}");
        }

        if (filter.ApplicationStatus is { } status)
        {
            filters.Add($"Status da candidatura: {status.ToDescription()} (afeta apenas números de candidatura)");
        }

        return filters;
    }

    private static string LocalDate(DateTimeOffset instant, TimeZoneInfo timeZone)
        => TimeZoneInfo.ConvertTime(instant, timeZone).ToString(DateFormat);

    private static IReadOnlyCollection<UF>? NormalizeStates(IReadOnlyCollection<UF>? states)
        => Normalize(states, UF.NaoSelecionado);

    private static IReadOnlyCollection<JobAreaEnum>? NormalizeAreas(IReadOnlyCollection<JobAreaEnum>? areas)
        => Normalize(areas, JobAreaEnum.NaoSelecionado);

    private static ApplicationStatusEnum? NormalizeStatus(ApplicationStatusEnum? status)
        => status == ApplicationStatusEnum.NaoSelecionado ? null : status;

    private static IReadOnlyCollection<TEnum>? Normalize<TEnum>(IReadOnlyCollection<TEnum>? values, TEnum neutral)
        where TEnum : struct, Enum
    {
        if (values is null || values.Count == 0)
        {
            return null;
        }

        var cleaned = values.Where(value => !value.Equals(neutral)).Distinct().ToArray();
        return cleaned.Length == 0 ? null : cleaned;
    }
}

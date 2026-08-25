namespace EmpregaNet.Application.Dashboard.ViewModel;

public sealed class DashboardOverviewViewModel
{
    public required DashboardMetaViewModel Meta { get; init; }
    public required IReadOnlyList<DashboardKpiViewModel> Kpis { get; init; }

    public required DashboardFunnelViewModel Funnel { get; init; }
}

public sealed class DashboardKpiViewModel
{
    public required string Key { get; init; }
    public required string Label { get; init; }
    public decimal? Value { get; init; }
    public string? Unit { get; init; }
    public decimal? PreviousValue { get; init; }
    public decimal? ChangePercent { get; init; }
    public required string Trend { get; init; }
    public required string Hint { get; init; }
    public required bool IsPeriodScoped { get; init; }
}

public sealed record DashboardFunnelViewModel(
    IReadOnlyList<DashboardFunnelStageViewModel> Stages,
    decimal? ConversionRate,
    string Note);

public sealed record DashboardFunnelStageViewModel(
    string Key,
    string Label,
    int Value,
    decimal? ShareOfPrevious,
    decimal? ShareOfEntry);

public sealed record DashboardInsightViewModel(
    string Code,
    string Category,
    string Tone,
    string Severity,
    string Title,
    string Message);

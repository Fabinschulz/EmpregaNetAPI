namespace EmpregaNet.Application.Dashboard.ViewModel;

public sealed class DashboardMetaViewModel
{
    public required string Period { get; init; }
    public required string PeriodLabel { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public required string FromUtc { get; init; }
    public required string ToUtcExclusive { get; init; }
    public required int Days { get; init; }
    public required string PreviousFrom { get; init; }
    public required string PreviousTo { get; init; }
    public string Timezone { get; init; } = "America/Sao_Paulo";
    public required string GeneratedAt { get; init; }
    public required DashboardScopeViewModel Scope { get; init; }
    public IReadOnlyList<string> AppliedFilters { get; init; } = [];
    public IReadOnlyList<DashboardUnavailableMetricViewModel> Unavailable { get; init; } = [];
}

public sealed record DashboardScopeViewModel(string Level, long? CompanyId, string? CompanyName);
public sealed record DashboardUnavailableMetricViewModel(string Metric, string Label, string Reason);

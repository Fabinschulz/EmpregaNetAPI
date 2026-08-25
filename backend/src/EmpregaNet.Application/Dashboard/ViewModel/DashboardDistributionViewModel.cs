namespace EmpregaNet.Application.Dashboard.ViewModel;

public sealed class DashboardDistributionViewModel
{
    public required DashboardMetaViewModel Meta { get; init; }
    public required DashboardBreakdownViewModel ApplicationsByStatus { get; init; }
    public required DashboardBreakdownViewModel ApplicationsByArea { get; init; }
    public required DashboardBreakdownViewModel JobsByArea { get; init; }
}

public sealed record DashboardBreakdownViewModel(
    IReadOnlyList<DashboardBreakdownItemViewModel> Items,
    int Total,
    int Categorized,
    string? Note);
public sealed record DashboardBreakdownItemViewModel(string Key, string Label, int Value, decimal Share);

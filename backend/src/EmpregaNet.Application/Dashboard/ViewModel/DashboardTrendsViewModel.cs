namespace EmpregaNet.Application.Dashboard.ViewModel;

public sealed class DashboardTrendsViewModel
{
    public required DashboardMetaViewModel Meta { get; init; }

    public required string Granularity { get; init; }
    public required string GranularityLabel { get; init; }
    public required IReadOnlyList<DashboardSeriesViewModel> Series { get; init; }
}

public sealed record DashboardSeriesViewModel(
    string Key,
    string Label,
    int Total,
    IReadOnlyList<DashboardSeriesPointViewModel> Points);

public sealed record DashboardSeriesPointViewModel(string Date, string Label, int Value);

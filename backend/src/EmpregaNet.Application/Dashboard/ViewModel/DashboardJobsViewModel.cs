namespace EmpregaNet.Application.Dashboard.ViewModel;

/// <summary>
/// Ranking de desempenho das vagas.
/// </summary>
public sealed class DashboardJobsViewModel
{
    public required DashboardMetaViewModel Meta { get; init; }
    public required string Ranking { get; init; }
    public required string RankingLabel { get; init; }
    public required bool OnlyActive { get; init; }
    public required decimal AverageApplicationsPerJob { get; init; }
    public required int JobsInAverage { get; init; }
    public required IReadOnlyList<DashboardJobPerformanceViewModel> Items { get; init; }
}

public sealed class DashboardJobPerformanceViewModel
{
    public required long Id { get; init; }
    public required string Title { get; init; }
    public required long CompanyId { get; init; }
    public required string CompanyName { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Area { get; init; }
    public required string AreaLabel { get; init; }
    public required bool IsActive { get; init; }
    public required string StatusLabel { get; init; }
    public required string PublishedAt { get; init; }
    public required int DaysActive { get; init; }
    public required int Applications { get; init; }
    public required int TotalApplications { get; init; }
    public string? LastApplicationAt { get; init; }
    public int? DaysSinceLastApplication { get; init; }
    public decimal? PerformanceVsAverage { get; init; }
    public required string Performance { get; init; }
}

using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Common.Dashboard;

public sealed record DashboardPeriodCounters(
    int NewJobs,
    int NewApplications,
    int NewCandidates);

public sealed record DashboardComparedCounters(
    DashboardPeriodCounters Current,
    DashboardPeriodCounters Previous);

public sealed record DashboardStatusComparison(ApplicationStatusEnum Status, int Current, int Previous);

public sealed record DashboardTotals(
    int ActiveJobs,
    int CompaniesWithActiveJobs);

public sealed record DashboardDailyPoint(DateOnly Day, int Count);

public sealed record DashboardEnumCount<TEnum>(TEnum Value, int Count) where TEnum : struct, Enum;
public sealed record DashboardJobPerformance(
    long JobId,
    string Title,
    long CompanyId,
    string CompanyName,
    string City,
    UF State,
    JobAreaEnum Area,
    bool IsActive,
    DateTimeOffset PublishedAt,
    int Applications,
    int TotalApplications,
    DateTimeOffset? LastApplicationAt);

public enum DashboardJobRankingEnum
{
    MostApplications = 0,
    FewestApplications = 1,
    MostRecent = 2
}

public sealed record DashboardJobHealth(
    int ActiveJobs,
    int JobsWithoutApplications,
    int StaleJobs,
    int ClosedJobsInPeriod);

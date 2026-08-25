using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Common.Dashboard;

public sealed record DashboardScope(long? CompanyId)
{
    public static readonly DashboardScope Platform = new((long?)null);
    public bool IsPlatformWide => CompanyId is null;
}

public sealed record DashboardDateRange(DateTimeOffset FromUtc, DateTimeOffset ToUtcExclusive)
{
    public TimeSpan Duration => ToUtcExclusive - FromUtc;
    public DashboardDateRange Previous() => new(FromUtc - Duration, FromUtc);
}

public sealed record DashboardFilter(
    DashboardScope Scope,
    DashboardDateRange Range,
    TimeSpan LocalOffset,
    IReadOnlyCollection<UF>? States = null,
    IReadOnlyCollection<JobAreaEnum>? Areas = null,
    ApplicationStatusEnum? ApplicationStatus = null)
{
    public DashboardFilter For(DashboardDateRange range) => this with { Range = range };
    public bool HasStates => States is { Count: > 0 };
    public bool HasAreas => Areas is { Count: > 0 };
}

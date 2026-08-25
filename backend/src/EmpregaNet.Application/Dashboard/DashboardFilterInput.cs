using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Dashboard;

public sealed record DashboardFilterInput(
    DashboardPeriodEnum Period = DashboardPeriodEnum.Last30Days,
    DateOnly? From = null,
    DateOnly? To = null,
    long? CompanyId = null,
    IReadOnlyCollection<UF>? States = null,
    IReadOnlyCollection<JobAreaEnum>? Areas = null,
    ApplicationStatusEnum? ApplicationStatus = null);

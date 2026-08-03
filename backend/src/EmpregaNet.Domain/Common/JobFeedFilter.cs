using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Common;

/// <summary>
/// Critérios de consulta do feed público de vagas.
/// </summary>
public sealed record JobFeedFilter(
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
    DateTimeOffset? PublishedAfter = null,
    JobFeedSortEnum Sort = JobFeedSortEnum.Recent,
    int Page = 1,
    int Size = 20)
{
    public bool HasSearch => !string.IsNullOrWhiteSpace(Search);
    public bool HasSalaryBound => SalaryMin.HasValue || SalaryMax.HasValue;
}

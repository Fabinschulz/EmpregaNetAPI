using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Common;

/// <summary>
/// Linha do feed já resolvida pelo repositório: vaga + dados da empresa + contagem de candidaturas.
/// </summary>
public sealed record JobFeedProjection(
    long Id,
    string Title,
    string? Summary,
    JobFeedCompany Company,
    JobFeedLocation Location,
    JobFeedSalary Salary,
    JobTypeEnum JobType,
    WorkModelEnum WorkModel,
    WorkShiftEnum WorkShift,
    ExperienceLevelEnum ExperienceLevel,
    JobAreaEnum Area,
    bool IsPcdFriendly,
    List<string> Requirements,
    List<string> Benefits,
    DateTimeOffset PublishedAt,
    int ApplicationsCount,
    bool IsActive);

public sealed record JobFeedCompany(long Id, string Name);
public sealed record JobFeedLocation(string City, UF State, string Country);

/// <summary>Faixa salarial da vaga. <paramref name="Disclosed"/> falso significa "a combinar".</summary>
public sealed record JobFeedSalary(decimal? Min, decimal? Max, bool Disclosed);

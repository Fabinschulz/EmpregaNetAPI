using System.Diagnostics.CodeAnalysis;
using EmpregaNet.Domain.Common;

namespace EmpregaNet.Application.Jobs.ViewModel;

/// <summary>
/// Cartão do feed de vagas.
/// </summary>
public sealed class JobFeedItemViewModel
{
    public long Id { get; init; }
    public required string Title { get; init; }
    public string? Summary { get; init; }
    public required JobFeedCompanyViewModel Company { get; init; }
    public required JobFeedLocationViewModel Location { get; init; }
    public required JobFeedSalaryViewModel Salary { get; init; }
    public required string JobType { get; init; }
    public required string WorkModel { get; init; }
    public required string WorkShift { get; init; }
    public required string ExperienceLevel { get; init; }
    public required string Area { get; init; }
    public bool IsPcdFriendly { get; init; }
    public required IReadOnlyList<string> Requirements { get; init; }
    public required IReadOnlyList<string> Benefits { get; init; }
    public DateTimeOffset PublishedAt { get; init; } = DateTimeOffset.UtcNow;
    public int ApplicationsCount { get; init; }
    public bool IsActive { get; init; }
}

public sealed class JobFeedCompanyViewModel
{
    public long Id { get; init; }
    public required string Name { get; init; }
    public string? LogoUrl { get; init; }
}

public sealed class JobFeedLocationViewModel
{
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Country { get; init; }
}

public sealed class JobFeedSalaryViewModel
{
    public decimal? Min { get; init; }
    public decimal? Max { get; init; }

    /// <summary><c>false</c> significa "a combinar".</summary>
    public bool Disclosed { get; init; }
}

[ExcludeFromCodeCoverage]
public static class JobFeedMapper
{
    public static JobFeedItemViewModel ToFeedItem(this JobFeedProjection projection)
    {
        return new JobFeedItemViewModel
        {
            Id = projection.Id,
            Title = projection.Title,
            Summary = projection.Summary,
            Company = new JobFeedCompanyViewModel
            {
                Id = projection.Company.Id,
                Name = projection.Company.Name,
                LogoUrl = null
            },
            Location = new JobFeedLocationViewModel
            {
                City = projection.Location.City,
                State = projection.Location.State.ToString(),
                Country = projection.Location.Country
            },
            Salary = new JobFeedSalaryViewModel
            {
                Min = projection.Salary.Min,
                Max = projection.Salary.Max,
                Disclosed = projection.Salary.Disclosed
            },
            JobType = projection.JobType.ToString(),
            WorkModel = projection.WorkModel.ToString(),
            WorkShift = projection.WorkShift.ToString(),
            ExperienceLevel = projection.ExperienceLevel.ToString(),
            Area = projection.Area.ToString(),
            IsPcdFriendly = projection.IsPcdFriendly,
            Requirements = projection.Requirements,
            Benefits = projection.Benefits,
            PublishedAt = projection.PublishedAt,
            ApplicationsCount = projection.ApplicationsCount,
            IsActive = projection.IsActive
        };
    }
}

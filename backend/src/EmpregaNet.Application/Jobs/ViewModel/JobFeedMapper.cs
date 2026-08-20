using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Common;

namespace EmpregaNet.Application.Jobs.ViewModel;

public static class JobFeedMapper
{
    private const int SummaryMaxLength = 280;

    public static JobFeedItemViewModel ToFeedItem(this JobFeedProjection projection)
    {
        return new JobFeedItemViewModel
        {
            Id = projection.Id,
            Title = projection.Title,
            Summary = ResolveSummary(projection),
            Company = ToCompany(projection.Company),
            Location = ToLocation(projection.Location),
            Salary = ToSalary(projection.Salary),
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

    private static string? ResolveSummary(JobFeedProjection projection)
    {
        var summary = projection.Summary?.Trim();
        if (!string.IsNullOrEmpty(summary))
        {
            return summary;
        }

        var excerpt = projection.DescriptionExcerpt.ToExcerpt(SummaryMaxLength, projection.DescriptionTruncated);

        return excerpt.Length == 0 ? null : excerpt;
    }

    private static JobFeedCompanyViewModel ToCompany(JobFeedCompany company)
        => new() { Id = company.Id, Name = company.Name, LogoUrl = null };

    private static JobFeedLocationViewModel ToLocation(JobFeedLocation location)
        => new()
        {
            City = location.City,
            State = location.State.ToString(),
            Country = location.Country
        };

    private static JobFeedSalaryViewModel ToSalary(JobFeedSalary salary)
        => new() { Min = salary.Min, Max = salary.Max, Disclosed = salary.Disclosed };
}

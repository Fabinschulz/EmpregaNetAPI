using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using System.Diagnostics.CodeAnalysis;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Utils.Helpers;

namespace EmpregaNet.Application.Jobs.ViewModel;

public sealed class JobViewModel : BaseViewModel
{
    public long Id { get; set; }
    public required string Title { get; set; }
    public string? Summary { get; set; }
    public required string Description { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public bool SalaryDisclosed { get; set; }
    public JobTypeEnum JobType { get; set; }
    public WorkModelEnum WorkModel { get; set; }
    public WorkShiftEnum WorkShift { get; set; }
    public ExperienceLevelEnum ExperienceLevel { get; set; }
    public JobAreaEnum Area { get; set; }
    public bool IsPcdFriendly { get; set; }
    public required string City { get; set; }
    public UF State { get; set; }
    public required string Country { get; set; }
    public required IReadOnlyList<string> Requirements { get; set; }
    public required IReadOnlyList<string> Benefits { get; set; }
    public required string PublicationDate { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public long CompanyId { get; set; }
    public bool IsActive { get; set; }
}

[ExcludeFromCodeCoverage]
public static class JobMapper
{
    public static JobViewModel ToViewModel(this Job entity)
    {
        return new JobViewModel
        {
            Id = entity.Id,
            CompanyId = entity.CompanyId,
            Title = entity.Title,
            Summary = entity.Summary,
            Description = entity.Description,
            SalaryMin = entity.SalaryMin,
            SalaryMax = entity.SalaryMax,
            SalaryDisclosed = entity.SalaryDisclosed,
            JobType = entity.JobType,
            WorkModel = entity.WorkModel,
            WorkShift = entity.WorkShift,
            ExperienceLevel = entity.ExperienceLevel,
            Area = entity.Area,
            IsPcdFriendly = entity.IsPcdFriendly,
            City = entity.Location.City,
            State = entity.Location.State,
            Country = entity.Location.Country,
            Requirements = entity.Requirements,
            Benefits = entity.Benefits,
            IsActive = entity.IsActive,
            PublishedAt = entity.PublishedAt,
            PublicationDate = BrasiliaTime.Format(entity.PublishedAt),
            CreatedAtUtc = entity.CreatedAt,
            UpdatedAtUtc = entity.UpdatedAt,
            DeletedAtUtc = entity.DeletedAt,
            IsDeleted = entity.IsDeleted
        };
    }
}

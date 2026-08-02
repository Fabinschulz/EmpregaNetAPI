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
    public required string Description { get; set; }
    public decimal Salary { get; set; }
    public JobTypeEnum JobType { get; set; }
    public required string PublicationDate { get; set; }
    public long CompanyId { get; set; }

    /// <summary>
    /// Vaga aberta a candidaturas. <c>false</c> após <c>PUT /api/jobs/{id}/close</c>.
    /// </summary>
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
            Description = entity.Description,
            Salary = entity.Salary,
            JobType = entity.JobType,
            IsActive = entity.IsActive,
            PublicationDate =  RandomHelpers.FormatToBrasiliaTime(entity.PublishedAt),
            CreatedAtUtc = entity.CreatedAt,
            UpdatedAtUtc = entity.UpdatedAt,
            DeletedAtUtc = entity.DeletedAt,
            IsDeleted = entity.IsDeleted
        };
    }
}

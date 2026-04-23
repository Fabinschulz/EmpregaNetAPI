using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace EmpregaNet.Application.JobApplications.ViewModel;

public sealed class JobApplicationViewModel : BaseViewModel
{
    public long Id { get; set; }
    public long JobId { get; set; }
    public long UserId { get; set; }
    public ApplicationStatusEnum Status { get; set; }
    public string AppliedAt { get; set; } = string.Empty;
}

[ExcludeFromCodeCoverage]
public static class JobApplicationMapper
{
    public static JobApplicationViewModel ToViewModel(this JobApplication entity)
    {
        return new JobApplicationViewModel
        {
            Id = entity.Id,
            JobId = entity.JobId,
            UserId = entity.UserId,
            Status = entity.Status,
            AppliedAt = RandomHelpers.FormatToBrasiliaTime(entity.AppliedAt),
            CreatedAtUtc = entity.CreatedAt,
            UpdatedAtUtc = entity.UpdatedAt,
            DeletedAtUtc = entity.DeletedAt,
            IsDeleted = entity.IsDeleted
        };
    }
}

using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace EmpregaNet.Application.JobApplications.ViewModel;

public sealed class JobApplicationViewModel : BaseViewModel
{
    public long Id { get; set; }
    public long JobId { get; set; }
    public required JobApplicationCandidateViewModel Candidate { get; set; }
    public ApplicationStatusEnum Status { get; set; }
    public string AppliedAt { get; set; } = string.Empty;
}

/// <summary>Quem se candidatou, para as telas de recrutamento.</summary>
public sealed class JobApplicationCandidateViewModel
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}

[ExcludeFromCodeCoverage]
public static class JobApplicationMapper
{
    public static JobApplicationViewModel ToViewModel(this JobApplicationProjection proj)
    {
        return new JobApplicationViewModel
        {
            Id = proj.Id,
            JobId = proj.JobId,
            Candidate = new JobApplicationCandidateViewModel
            {
                Id = proj.Candidate.Id,
                Name = proj.Candidate.Name,
                Email = proj.Candidate.Email,
                IsDeleted = proj.Candidate.IsDeleted
            },
            Status = proj.Status,
            AppliedAt = BrasiliaTime.Format(proj.AppliedAt),
            CreatedAtUtc = proj.CreatedAt,
            UpdatedAtUtc = proj.UpdatedAt,
            DeletedAtUtc = proj.DeletedAt,
            IsDeleted = proj.IsDeleted
        };
    }
}

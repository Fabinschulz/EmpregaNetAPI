using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Common;

public sealed record JobApplicationProjection(
    long Id,
    long JobId,
    JobApplicationCandidate Candidate,
    ApplicationStatusEnum Status,
    DateTimeOffset AppliedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? DeletedAt,
    bool IsDeleted);

public sealed record JobApplicationCandidate(long Id, string Name, string Email, bool IsDeleted);

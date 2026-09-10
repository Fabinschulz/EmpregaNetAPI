using EmpregaNet.Application.Abstraction;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.JobApplications.Events;

public enum JobApplicationNotificationReason
{
    /// <summary> Candidatura criada.</summary>
    Applied,
    /// <summary> Status da candidatura alterado.</summary>
    StatusChanged,
    /// <summary> Candidatura encerrada pela empresa.</summary>
    JobClosed,
    /// <summary> Candidatura cancelada pelo candidato.</summary>
    CanceledByCandidate
}

/// <summary>
/// Uma candidatura mudou de estado. Publicado <b>depois</b> do commit, pelo
/// <c>NotificationDispatchBehavior</c>.
/// </summary>
/// <remarks>
/// Só transporta identificadores e estados, nada de e-mail, nome ou título de vaga. Quem consome
/// resolve o que precisa a partir do <paramref name="JobApplicationId"/>, o que mantém o destinatário
/// derivado do agregado e nunca de dado vindo da requisição.
/// </remarks>
/// <param name="JobApplicationId">Candidatura afectada.</param>
/// <param name="JobId">Vaga da candidatura.</param>
/// <param name="CandidateUserId">Dono da candidatura; é a ele que a notificação se destina.</param>
/// <param name="PreviousStatus">Estado antes da mudança.</param>
/// <param name="NewStatus">Estado depois da mudança.</param>
/// <param name="OccurredAt">Momento da mudança, em UTC.</param>
/// <param name="Reason">O que provocou a mudança.</param>
public sealed record JobApplicationStatusChanged(
    long JobApplicationId,
    long JobId,
    long CandidateUserId,
    ApplicationStatusEnum PreviousStatus,
    ApplicationStatusEnum NewStatus,
    DateTimeOffset OccurredAt,
    JobApplicationNotificationReason Reason) : IIdentifiableNotification
{
    public string DeduplicationKey => $"{JobApplicationId}:{(int)NewStatus}:{(int)Reason}";
}

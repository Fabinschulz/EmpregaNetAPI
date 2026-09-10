namespace EmpregaNet.Domain.Common;

/// <summary>
/// Dados necessários para compor uma notificação de andamento de uma candidatura em uma única consulta.
/// </summary>
/// <remarks>
/// Este projection evita carregar <c>JobApplication</c>, <c>Job</c>, <c>Company</c> e <c>User</c>
/// completos apenas para montar o texto da mensagem, além de evitar consultas N+1 em quatro
/// entidades por candidatura notificada. Como a consulta é indexada pela candidatura, o destinatário
/// é sempre obtido do registro e não de dados da requisição.
/// </remarks>
public sealed record JobApplicationNotificationProjection(
    long JobApplicationId,
    long JobId,
    string JobTitle,
    string CompanyName,
    long CandidateId,
    string CandidateName,
    string CandidateEmail);

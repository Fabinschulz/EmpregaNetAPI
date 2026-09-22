using EmpregaNet.Application.JobApplications.Events;

namespace EmpregaNet.Application.Abstraction;

/// <summary>
/// Efeito de arrasto de encerrar uma vaga sobre as candidaturas que ainda estavam em aberto.
/// </summary>
public interface IJobClosureCascade
{
    /// <summary>
    /// Cancela as candidaturas em aberto da vaga e enfileira a notificação de cada uma.
    /// </summary>
    /// <param name="jobId">Vaga encerrada.</param>
    /// <param name="closedAt">Instante do encerramento, em UTC, comum a todas as notificações.</param>
    /// <param name="reason">O que encerrou a vaga; decide o texto que o candidato recebe.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <returns>Quantas candidaturas foram canceladas.</returns>
    Task<int> CancelOpenApplicationsAsync(
        long jobId,
        DateTimeOffset closedAt,
        JobApplicationNotificationReason reason,
        CancellationToken cancellationToken);
}

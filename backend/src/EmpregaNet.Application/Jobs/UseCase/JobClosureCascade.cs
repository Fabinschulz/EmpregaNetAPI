using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.JobApplications.Events;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Jobs.UseCase;

/// <inheritdoc cref="IJobClosureCascade"/>
public sealed class JobClosureCascade : IJobClosureCascade
{
    /// <summary>
    /// Acima disto, o encerramento escreve em muitas linhas dentro de uma transacção e vale registar.
    /// </summary>
    private const int LargeBatchWarningThreshold = 200;

    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IDomainEventQueue _domainEvents;
    private readonly ILogger<JobClosureCascade> _logger;

    public JobClosureCascade(
        IJobApplicationRepository jobApplicationRepository,
        IDomainEventQueue domainEvents,
        ILogger<JobClosureCascade> logger)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _domainEvents = domainEvents;
        _logger = logger;
    }

    public async Task<int> CancelOpenApplicationsAsync(
        long jobId,
        DateTimeOffset closedAt,
        JobApplicationNotificationReason reason,
        CancellationToken cancellationToken)
    {
        var openApplications = await _jobApplicationRepository.GetOpenByJobIdAsync(jobId, cancellationToken);

        if (openApplications.Count >= LargeBatchWarningThreshold)
        {
            _logger.LogWarning(
                "Encerramento da vaga {JobId} vai cancelar {Count} candidaturas na mesma transação.",
                jobId,
                openApplications.Count);
        }

        foreach (var application in openApplications)
        {
            var previousStatus = application.Status;

            application.ChangeStatus(ApplicationStatusEnum.Canceled);

            _domainEvents.Enqueue(new JobApplicationStatusChanged(
                JobApplicationId: application.Id,
                JobId: application.JobId,
                CandidateUserId: application.UserId,
                PreviousStatus: previousStatus,
                NewStatus: application.Status,
                OccurredAt: closedAt,
                Reason: reason));
        }

        return openApplications.Count;
    }
}

using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Events;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Jobs.Commands;

public sealed record CloseJobCommand(long JobId) : IRequest<CloseJobResult>, ITransactional;

/// <summary>
/// Resultado do encerramento de uma vaga, incluindo o número de candidaturas que foram canceladas por arrasto.
/// </summary>
/// <param name="JobId">Vaga encerrada.</param>
/// <param name="ClosedAt">Instante do encerramento, em UTC.</param>
/// <param name="AffectedApplications">Candidaturas em aberto que foram canceladas por arrasto.</param>
public sealed record CloseJobResult(long JobId, DateTimeOffset ClosedAt, int AffectedApplications);

public sealed class CloseJobHandler : IRequestHandler<CloseJobCommand, CloseJobResult>
{
    /// <summary>
    /// Acima disto, o encerramento escreve em muitas linhas dentro de uma transacção e vale registar.
    /// </summary>
    private const int LargeBatchWarningThreshold = 200;

    private readonly IJobRepository _jobRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IDomainEventQueue _domainEvents;
    private readonly IValidator<CloseJobCommand> _validator;
    private readonly ILogger<CloseJobHandler> _logger;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IJobEmployerAccess _jobEmployerAccess;

    public CloseJobHandler(
        IJobRepository jobRepository,
        IJobApplicationRepository jobApplicationRepository,
        IDomainEventQueue domainEvents,
        IValidator<CloseJobCommand> validator,
        ILogger<CloseJobHandler> logger,
        IHttpCurrentUser httpCurrentUser,
        IJobEmployerAccess jobEmployerAccess)
    {
        _jobRepository = jobRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _domainEvents = domainEvents;
        _validator = validator;
        _logger = logger;
        _httpCurrentUser = httpCurrentUser;
        _jobEmployerAccess = jobEmployerAccess;
    }

    public async Task<CloseJobResult> Handle(CloseJobCommand request, CancellationToken cancellationToken)
    {
        RecruitmentAccess.EnsureRecruitmentStaff(_httpCurrentUser);

        _logger.LogInformation("Iniciando encerramento da vaga {JobId}", request.JobId);

        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.JobId),
                $"Vaga com ID '{request.JobId}' não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        if (!job.IsActive)
        {
            throw new ValidationAppException(
                nameof(request.JobId),
                "A vaga já está encerrada.",
                DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        }

        await _jobEmployerAccess.EnsureCanManageCompanyAsync(job.CompanyId, cancellationToken);

        job.Close();
        await _jobRepository.UpdateAsync(job, cancellationToken);

        var closedAt = DateTimeOffset.UtcNow;
        var openApplications = await _jobApplicationRepository.GetOpenByJobIdAsync(request.JobId, cancellationToken);

        if (openApplications.Count >= LargeBatchWarningThreshold)
        {
            _logger.LogWarning(
                "Encerramento da vaga {JobId} vai cancelar {Count} candidaturas na mesma transação.",
                request.JobId,
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
                Reason: JobApplicationNotificationReason.JobClosed));
        }

        _logger.LogInformation(
            "Vaga {JobId} encerrada. Candidaturas canceladas por arrasto: {Count}.",
            request.JobId,
            openApplications.Count);

        return new CloseJobResult(request.JobId, closedAt, openApplications.Count);
    }
}

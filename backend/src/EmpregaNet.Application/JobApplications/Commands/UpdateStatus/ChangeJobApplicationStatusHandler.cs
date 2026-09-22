using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.JobApplications.Events;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace EmpregaNet.Application.JobApplications.Commands;

public sealed record ChangeJobApplicationStatusCommand(
    [EnumDataType(typeof(ApplicationStatusEnum))]
    string Status
) : IRequest<JobApplicationViewModel>, ITransactional;

public sealed class ChangeJobApplicationStatusCommandHandler :
    IRequestHandler<UpdateCommand<ChangeJobApplicationStatusCommand, JobApplicationViewModel>, JobApplicationViewModel>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IJobEmployerAccess _jobEmployerAccess;
    private readonly IJobClosureCascade _closureCascade;
    private readonly IDomainEventQueue _domainEvents;
    private readonly ILogger<ChangeJobApplicationStatusCommandHandler> _logger;

    public ChangeJobApplicationStatusCommandHandler(
        IJobApplicationRepository jobApplicationRepository,
        IJobRepository jobRepository,
        IJobEmployerAccess jobEmployerAccess,
        IJobClosureCascade closureCascade,
        IDomainEventQueue domainEvents,
        ILogger<ChangeJobApplicationStatusCommandHandler> logger)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _jobRepository = jobRepository;
        _jobEmployerAccess = jobEmployerAccess;
        _closureCascade = closureCascade;
        _domainEvents = domainEvents;
        _logger = logger;
    }

    public async Task<JobApplicationViewModel> Handle(
        UpdateCommand<ChangeJobApplicationStatusCommand, JobApplicationViewModel> request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando status da candidatura {ApplicationId}", request.Id);

        var application = await _jobApplicationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (application is null || application.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.Id),
                $"Candidatura com ID '{request.Id}' não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        // Linha da vaga bloqueada até ao commit: é aqui que o contador de posições pode mudar, e
        // duas aprovações simultâneas na mesma vaga não podem ler o mesmo saldo.
        var job = await _jobRepository.GetByIdForUpdateAsync(application.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            throw ValidationAppException.ForBusinessRule(
                "Vaga associada à candidatura não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        await _jobEmployerAccess.EnsureCanManageCompanyAsync(job.CompanyId, cancellationToken);

        if (!Enum.TryParse<ApplicationStatusEnum>(request.entity.Status, true, out var newStatus) ||
            newStatus == ApplicationStatusEnum.NaoSelecionado)
        {
            throw new ValidationAppException(
                nameof(request.entity.Status),
                "Status de candidatura inválido.",
                DomainErrorEnum.INVALID_PARAMS);
        }

        var previousStatus = application.Status;
        var heldPosition = JobApplication.HoldsPosition(previousStatus);
        var willHoldPosition = JobApplication.HoldsPosition(newStatus);

        try
        {
            application.ChangeStatus(newStatus);
        }
        catch (InvalidOperationException ex)
        {
            throw ValidationAppException.ForBusinessRule(ex.Message, DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        }

        var jobFilledUp = ApplyPositionEffect(job, heldPosition, willHoldPosition);

        if (heldPosition != willHoldPosition)
        {
            await _jobRepository.UpdateAsync(job, cancellationToken);
        }

        await _jobApplicationRepository.UpdateAsync(application, cancellationToken);

        _domainEvents.Enqueue(new JobApplicationStatusChanged(
            JobApplicationId: application.Id,
            JobId: application.JobId,
            CandidateUserId: application.UserId,
            PreviousStatus: previousStatus,
            NewStatus: application.Status,
            OccurredAt: DateTimeOffset.UtcNow,
            Reason: JobApplicationNotificationReason.StatusChanged));

        if (jobFilledUp)
        {
            await CascadeJobFilledAsync(job, cancellationToken);
        }

        var updated = await _jobApplicationRepository.GetProjectionByIdAsync(request.Id, cancellationToken);
        if (updated is null)
        {
            throw new ValidationAppException(
                nameof(request.Id),
                $"Candidatura não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        return updated.ToViewModel();
    }

    /// <summary>
    /// Reflecte na vaga o efeito da transição sobre as posições.
    /// </summary>
    /// <returns><c>true</c> se esta transição encheu a vaga e a encerrou.</returns>
    private static bool ApplyPositionEffect(Job job, bool heldPosition, bool willHoldPosition)
    {
        if (heldPosition == willHoldPosition) return false;

        try
        {
            if (willHoldPosition) return job.FillPosition();

            job.ReleasePosition();
            return false;
        }
        catch (InvalidOperationException ex)
        {
            throw ValidationAppException.ForBusinessRule(ex.Message, DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        }
    }

    /// <summary>
    /// A vaga encheu: sai do feed e as candidaturas ainda em aberto são canceladas, tal como no
    /// encerramento manual, mas com a razão que explica ao candidato que a vaga ficou completa.
    /// </summary>
    private async Task CascadeJobFilledAsync(Job job, CancellationToken cancellationToken)
    {
        var affected = await _closureCascade.CancelOpenApplicationsAsync(
            job.Id,
            job.ClosedAt!.Value,
            JobApplicationNotificationReason.JobFilled,
            cancellationToken);

        _logger.LogInformation(
            "Vaga {JobId} encerrada automaticamente: {Positions} posição(ões) preenchida(s). " +
            "Candidaturas canceladas por arrasto: {Count}.",
            job.Id,
            job.Positions,
            affected);
    }
}

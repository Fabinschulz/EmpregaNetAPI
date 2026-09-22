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
    private readonly IJobRepository _jobRepository;
    private readonly IJobClosureCascade _closureCascade;
    private readonly IValidator<CloseJobCommand> _validator;
    private readonly ILogger<CloseJobHandler> _logger;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IJobEmployerAccess _jobEmployerAccess;

    public CloseJobHandler(
        IJobRepository jobRepository,
        IJobClosureCascade closureCascade,
        IValidator<CloseJobCommand> validator,
        ILogger<CloseJobHandler> logger,
        IHttpCurrentUser httpCurrentUser,
        IJobEmployerAccess jobEmployerAccess)
    {
        _jobRepository = jobRepository;
        _closureCascade = closureCascade;
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

        var closedAt = job.ClosedAt!.Value;

        var affected = await _closureCascade.CancelOpenApplicationsAsync(
            request.JobId,
            closedAt,
            JobApplicationNotificationReason.JobClosed,
            cancellationToken);

        _logger.LogInformation(
            "Vaga {JobId} encerrada manualmente. Candidaturas canceladas por arrasto: {Count}.",
            request.JobId,
            affected);

        return new CloseJobResult(request.JobId, closedAt, affected);
    }
}

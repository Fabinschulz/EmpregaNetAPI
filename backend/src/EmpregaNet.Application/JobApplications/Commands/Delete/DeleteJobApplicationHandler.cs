using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.JobApplications.Commands;

/// <summary>
/// Remove uma candidatura (apenas recrutamento). Bloqueada quando o processo já chegou ao desfecho
/// <see cref="ApplicationStatusEnum.Finished"/> ("Concluída"): esse estado representa uma contratação
/// efetivada e é histórico que não deve ser apagável.
/// </summary>
public sealed class DeleteJobApplicationHandler : IRequestHandler<DeleteCommand<JobApplicationViewModel>, bool>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IJobEmployerAccess _jobEmployerAccess;
    private readonly ILogger<DeleteJobApplicationHandler> _logger;

    public DeleteJobApplicationHandler(
        IJobApplicationRepository jobApplicationRepository,
        IJobRepository jobRepository,
        IJobEmployerAccess jobEmployerAccess,
        ILogger<DeleteJobApplicationHandler> logger)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _jobRepository = jobRepository;
        _jobEmployerAccess = jobEmployerAccess;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCommand<JobApplicationViewModel> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando remoção da candidatura com ID: {Id}", request.Id);

        var application = await _jobApplicationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (application is null || application.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.Id),
                $"Candidatura com ID '{request.Id}' não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        var job = await _jobRepository.GetByIdAsync(application.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            throw ValidationAppException.ForBusinessRule(
                "Vaga associada à candidatura não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        await _jobEmployerAccess.EnsureCanManageCompanyAsync(job.CompanyId, cancellationToken);

        if (application.Status == ApplicationStatusEnum.Finished)
        {
            throw ValidationAppException.ForBusinessRule(
                "Não é possível excluir uma candidatura já concluída. Esse registro representa uma contratação efetivada.",
                DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
        }

        await _jobApplicationRepository.DeleteAsync(request.Id, cancellationToken);
        _logger.LogInformation("Candidatura removida com sucesso. ID: {Id}", request.Id);
        return true;
    }
}

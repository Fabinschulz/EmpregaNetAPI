using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
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
    private readonly IValidator<UpdateCommand<ChangeJobApplicationStatusCommand, JobApplicationViewModel>> _validator;
    private readonly ILogger<ChangeJobApplicationStatusCommandHandler> _logger;

    public ChangeJobApplicationStatusCommandHandler(
        IJobApplicationRepository jobApplicationRepository,
        IJobRepository jobRepository,
        IJobEmployerAccess jobEmployerAccess,
        IValidator<UpdateCommand<ChangeJobApplicationStatusCommand, JobApplicationViewModel>> validator,
        ILogger<ChangeJobApplicationStatusCommandHandler> logger)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _jobRepository = jobRepository;
        _jobEmployerAccess = jobEmployerAccess;
        _validator = validator;
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

        var job = await _jobRepository.GetByIdAsync(application.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(application.JobId),
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

        application.ChangeStatus(newStatus);
        await _jobApplicationRepository.UpdateAsync(application, cancellationToken);

        return application.ToViewModel();
    }
}

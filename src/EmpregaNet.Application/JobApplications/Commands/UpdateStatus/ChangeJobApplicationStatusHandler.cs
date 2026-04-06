using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
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
    private static readonly string[] AllowedRoles = ["Admin", "Recruiter", "Manager"];
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IValidator<UpdateCommand<ChangeJobApplicationStatusCommand, JobApplicationViewModel>> _validator;
    private readonly ILogger<ChangeJobApplicationStatusCommandHandler> _logger;

    public ChangeJobApplicationStatusCommandHandler(
        IJobApplicationRepository jobApplicationRepository,
        IHttpCurrentUser httpCurrentUser,
        IValidator<UpdateCommand<ChangeJobApplicationStatusCommand, JobApplicationViewModel>> validator,
        ILogger<ChangeJobApplicationStatusCommandHandler> logger)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _httpCurrentUser = httpCurrentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<JobApplicationViewModel> Handle(
        UpdateCommand<ChangeJobApplicationStatusCommand, JobApplicationViewModel> request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando status da candidatura {ApplicationId}", request.Id);

        EnsureCanManageApplications();

        var application = await _jobApplicationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (application is null || application.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.Id),
                $"Candidatura com ID '{request.Id}' não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

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

    private void EnsureCanManageApplications()
    {
        var user = _httpCurrentUser.GetContextUser();
        if (user is null)
        {
            throw new ValidationAppException(
                nameof(_httpCurrentUser.UserId),
                "Usuário autenticado não encontrado no contexto da requisição.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }

        var userRoles = user.UserToken.Claims
            .Where(c => c.Type.EndsWith("/claims/role", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!userRoles.Any(r => AllowedRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
        {
            throw new ValidationAppException(
                nameof(_httpCurrentUser.UserId),
                "Apenas usuários com perfil de recrutamento podem alterar o status da candidatura.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }
    }
}

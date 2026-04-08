using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.JobApplications.Commands;

public sealed record ApplyToJobCommand(long JobId);

public sealed class ApplyToJobHandler : IRequestHandler<CreateCommand<ApplyToJobCommand>, long>
{
    private static readonly string[] NonCandidateRoles = ["Admin", "Recruiter", "Manager", "Interviewer", "Analyst"];
    private readonly IJobRepository _jobRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IValidator<CreateCommand<ApplyToJobCommand>> _validator;
    private readonly ILogger<ApplyToJobHandler> _logger;

    public ApplyToJobHandler(
        IJobRepository jobRepository,
        IJobApplicationRepository jobApplicationRepository,
        IHttpCurrentUser httpCurrentUser,
        IValidator<CreateCommand<ApplyToJobCommand>> validator,
        ILogger<ApplyToJobHandler> logger)
    {
        _jobRepository = jobRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _httpCurrentUser = httpCurrentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<long> Handle(CreateCommand<ApplyToJobCommand> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando candidatura para vaga {JobId}", request.entity.JobId);

        var user = _httpCurrentUser.GetContextUser();
        if (user is null)
        {
            throw new ValidationAppException(
                nameof(_httpCurrentUser.UserId),
                "Usuário autenticado não encontrado no contexto da requisição.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }

        var userRoles = user.UserToken.GetRoleNames();

        if (userRoles.Any(r => NonCandidateRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
        {
            throw new ValidationAppException(
                nameof(_httpCurrentUser.UserId),
                "Apenas candidatos podem se candidatar para vagas.",
                DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
        }

        var job = await _jobRepository.GetByIdAsync(request.entity.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.entity.JobId),
                $"Vaga com ID '{request.entity.JobId}' não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        if (!job.IsActive)
        {
            throw new ValidationAppException(
                nameof(request.entity.JobId),
                "Não é possível se candidatar em uma vaga encerrada.",
                DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        }

        var alreadyApplied = await _jobApplicationRepository.ExistsAsync(request.entity.JobId, _httpCurrentUser.UserId, cancellationToken);
        if (alreadyApplied)
        {
            throw new ValidationAppException(
                nameof(request.entity.JobId),
                "Você já possui uma candidatura para esta vaga.",
                DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
        }

        var application = new JobApplication(request.entity.JobId, _httpCurrentUser.UserId);
        var created = await _jobApplicationRepository.CreateAsync(application, cancellationToken);
        _logger.LogInformation("Candidatura criada com sucesso. Id: {ApplicationId}", created.Id);
        return created.Id;
    }
}

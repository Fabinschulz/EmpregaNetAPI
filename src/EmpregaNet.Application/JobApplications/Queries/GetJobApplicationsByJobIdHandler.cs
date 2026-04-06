using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.JobApplications.Queries;

public sealed record GetJobApplicationsByJobIdQuery(long JobId, int Page, int Size, string? Status, string? OrderBy)
    : IRequest<ListDataPagination<JobApplicationViewModel>>, IPaginatedQuery;

public sealed class GetJobApplicationsByJobIdHandler :
    IRequestHandler<GetJobApplicationsByJobIdQuery, ListDataPagination<JobApplicationViewModel>>
{
    private static readonly string[] AllowedRoles = ["Admin", "Recruiter", "Manager"];
    private readonly IJobRepository _jobRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IValidator<GetJobApplicationsByJobIdQuery> _validator;
    private readonly ILogger<GetJobApplicationsByJobIdHandler> _logger;

    public GetJobApplicationsByJobIdHandler(
        IJobRepository jobRepository,
        IJobApplicationRepository jobApplicationRepository,
        IHttpCurrentUser httpCurrentUser,
        IValidator<GetJobApplicationsByJobIdQuery> validator,
        ILogger<GetJobApplicationsByJobIdHandler> logger)
    {
        _jobRepository = jobRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _httpCurrentUser = httpCurrentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ListDataPagination<JobApplicationViewModel>> Handle(
        GetJobApplicationsByJobIdQuery request,
        CancellationToken cancellationToken)
    {
        EnsureCanManageApplications();

        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.JobId),
                $"Vaga com ID '{request.JobId}' não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        _logger.LogInformation("Listando candidaturas da vaga {JobId}", request.JobId);
        var status = ParseStatus(request.Status);
        var result = await _jobApplicationRepository.GetByJobIdAsync(
            request.JobId,
            cancellationToken,
            request.Page,
            request.Size,
            status,
            request.OrderBy);

        var data = result.Data.Select(a => a.ToViewModel()).ToList();
        return new ListDataPagination<JobApplicationViewModel>(data, result.TotalItems, request.Page, request.Size);
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
                "Apenas usuários com perfil de recrutamento podem visualizar candidaturas por vaga.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }
    }

    private static ApplicationStatusEnum? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        if (!Enum.TryParse<ApplicationStatusEnum>(status, true, out var parsed) ||
            parsed == ApplicationStatusEnum.NaoSelecionado)
        {
            throw new ValidationAppException(
                nameof(status),
                "Status de candidatura inválido para filtro.",
                DomainErrorEnum.INVALID_QUERY_FILTER);
        }

        return parsed;
    }
}

using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.JobApplications.Queries;

public sealed record GetMyJobApplicationsQuery(int Page, int Size, string? Status, string? OrderBy)
    : IRequest<ListDataPagination<JobApplicationViewModel>>, IPaginatedQuery;

public sealed class GetMyJobApplicationsHandler :
    IRequestHandler<GetMyJobApplicationsQuery, ListDataPagination<JobApplicationViewModel>>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IValidator<GetMyJobApplicationsQuery> _validator;
    private readonly ILogger<GetMyJobApplicationsHandler> _logger;

    public GetMyJobApplicationsHandler(
        IJobApplicationRepository jobApplicationRepository,
        IHttpCurrentUser httpCurrentUser,
        IValidator<GetMyJobApplicationsQuery> validator,
        ILogger<GetMyJobApplicationsHandler> logger)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _httpCurrentUser = httpCurrentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ListDataPagination<JobApplicationViewModel>> Handle(
        GetMyJobApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _httpCurrentUser.UserId;
        _logger.LogInformation("Listando candidaturas do usuário {UserId}", userId);

        try
        {
            var status = ParseStatus(request.Status);
            var result = await _jobApplicationRepository.GetByUserIdAsync(
                userId,
                cancellationToken,
                request.Page,
                request.Size,
                status,
                request.OrderBy);

            var data = result.Data.Select(a => a.ToViewModel()).ToList();
            return new ListDataPagination<JobApplicationViewModel>(data, result.TotalItems, request.Page, request.Size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar minhas vagas aplicadas. Query: {@Query}", request);
            throw;
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

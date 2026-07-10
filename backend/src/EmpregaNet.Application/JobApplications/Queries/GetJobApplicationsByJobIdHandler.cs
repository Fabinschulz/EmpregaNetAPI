using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Abstraction;
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
    private readonly IJobRepository _jobRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IJobEmployerAccess _jobEmployerAccess;
    private readonly IValidator<GetJobApplicationsByJobIdQuery> _validator;
    private readonly ILogger<GetJobApplicationsByJobIdHandler> _logger;

    public GetJobApplicationsByJobIdHandler(
        IJobRepository jobRepository,
        IJobApplicationRepository jobApplicationRepository,
        IJobEmployerAccess jobEmployerAccess,
        IValidator<GetJobApplicationsByJobIdQuery> validator,
        ILogger<GetJobApplicationsByJobIdHandler> logger)
    {
        _jobRepository = jobRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _jobEmployerAccess = jobEmployerAccess;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ListDataPagination<JobApplicationViewModel>> Handle(
        GetJobApplicationsByJobIdQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.JobId),
                $"Vaga com ID '{request.JobId}' não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        await _jobEmployerAccess.EnsureCanManageCompanyAsync(job.CompanyId, cancellationToken);

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

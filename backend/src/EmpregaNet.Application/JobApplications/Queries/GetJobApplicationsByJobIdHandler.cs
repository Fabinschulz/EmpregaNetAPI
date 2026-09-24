using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.JobApplications.Queries;

/// <summary>
/// Candidaturas de uma vaga. <paramref name="Search"/> filtra por nome ou e-mail do candidato
/// (case-insensitive) antes da paginação.
/// </summary>
public sealed record GetJobApplicationsByJobIdQuery(
    long JobId, int Page, int Size, string? Status, string? OrderBy, string? Search = null)
    : IRequest<ListDataPagination<JobApplicationViewModel>>, IPaginatedQuery;

public sealed class GetJobApplicationsByJobIdHandler :
    IRequestHandler<GetJobApplicationsByJobIdQuery, ListDataPagination<JobApplicationViewModel>>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IJobEmployerAccess _jobEmployerAccess;
    private readonly ILogger<GetJobApplicationsByJobIdHandler> _logger;

    public GetJobApplicationsByJobIdHandler(
        IJobRepository jobRepository,
        IJobApplicationRepository jobApplicationRepository,
        IJobEmployerAccess jobEmployerAccess,
        ILogger<GetJobApplicationsByJobIdHandler> logger)
    {
        _jobRepository = jobRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _jobEmployerAccess = jobEmployerAccess;
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
        var status = ApplicationStatusParser.ParseOrNull(request.Status);
        var result = await _jobApplicationRepository.GetByJobIdAsync(
            request.JobId,
            cancellationToken,
            request.Page,
            request.Size,
            status,
            request.OrderBy,
            request.Search);

        var data = result.Data.Select(a => a.ToViewModel()).ToList();
        return new ListDataPagination<JobApplicationViewModel>(data, result.TotalItems, request.Page, request.Size);
    }
}

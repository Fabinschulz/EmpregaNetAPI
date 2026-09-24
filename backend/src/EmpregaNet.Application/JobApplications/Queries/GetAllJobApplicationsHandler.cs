using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.JobApplications.Queries;

public sealed class GetAllJobApplicationsHandler : IRequestHandler<GetAllJobApplicationsQuery, ListDataPagination<JobApplicationViewModel>>
{
    private readonly IJobApplicationRepository _repository;
    private readonly IJobEmployerAccess _jobEmployerAccess;
    private readonly ILogger<GetAllJobApplicationsHandler> _logger;

    public GetAllJobApplicationsHandler(
        IJobApplicationRepository repository,
        IJobEmployerAccess jobEmployerAccess,
        ILogger<GetAllJobApplicationsHandler> logger)
    {
        _repository = repository;
        _jobEmployerAccess = jobEmployerAccess;
        _logger = logger;
    }

    public async Task<ListDataPagination<JobApplicationViewModel>> Handle(GetAllJobApplicationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listando candidaturas (page: {Page}, size: {Size})", request.Page, request.Size);

        var companyScope = await _jobEmployerAccess.ResolveCompanyScopeAsync(cancellationToken);
        var status = ApplicationStatusParser.ParseOrNull(request.Status);

        try
        {
            var result = await _repository.GetAllWithCandidateAsync(
                cancellationToken,
                request.Page,
                request.Size,
                request.OrderBy,
                companyScope,
                status,
                request.Search);
            var data = result.Data.Select(a => a.ToViewModel()).ToList();
            return new ListDataPagination<JobApplicationViewModel>(data, result.TotalItems, request.Page, request.Size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar todas as vagas aplicadas. Query: {@Query}", request);
            throw;
        }
    }
}

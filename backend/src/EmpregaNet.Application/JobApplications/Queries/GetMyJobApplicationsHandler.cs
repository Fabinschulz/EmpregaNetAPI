using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.JobApplications.Queries;

public sealed record GetMyJobApplicationsQuery(int Page, int Size, string? Status, string? OrderBy)
    : IRequest<ListDataPagination<JobApplicationViewModel>>, IPaginatedQuery;

public sealed class GetMyJobApplicationsHandler :
    IRequestHandler<GetMyJobApplicationsQuery, ListDataPagination<JobApplicationViewModel>>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly ILogger<GetMyJobApplicationsHandler> _logger;

    public GetMyJobApplicationsHandler(
        IJobApplicationRepository jobApplicationRepository,
        IHttpCurrentUser httpCurrentUser,
        ILogger<GetMyJobApplicationsHandler> logger)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _httpCurrentUser = httpCurrentUser;
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
            var status = ApplicationStatusParser.ParseOrNull(request.Status);
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
}

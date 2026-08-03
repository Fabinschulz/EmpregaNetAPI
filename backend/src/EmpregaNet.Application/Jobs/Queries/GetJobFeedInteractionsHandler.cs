using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Jobs.Queries;

/// <summary>
/// Estado do utilizador atual sobre um conjunto de vagas do feed.
/// </summary>
public sealed record GetJobFeedInteractionsQuery(IReadOnlyCollection<long> JobIds)
    : IRequest<JobFeedInteractionsViewModel>;


public sealed class GetJobFeedInteractionsHandler
    : IRequestHandler<GetJobFeedInteractionsQuery, JobFeedInteractionsViewModel>
{
    private readonly IJobApplicationRepository _repository;
    private readonly IHttpCurrentUser _currentUser;
    private readonly ILogger<GetJobFeedInteractionsHandler> _logger;

    public GetJobFeedInteractionsHandler(
        IJobApplicationRepository repository,
        IHttpCurrentUser currentUser,
        ILogger<GetJobFeedInteractionsHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<JobFeedInteractionsViewModel> Handle(
        GetJobFeedInteractionsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.JobIds.Count == 0)
            return new JobFeedInteractionsViewModel { AppliedJobIds = Array.Empty<long>() };


        var appliedJobIds = await _repository.GetAppliedJobIdsAsync(
            _currentUser.UserId,
            request.JobIds,
            cancellationToken);

        _logger.LogInformation(
            "Interações do feed: {Applied} candidaturas em {Total} vagas consultadas.",
            appliedJobIds.Count, request.JobIds.Count);

        return new JobFeedInteractionsViewModel { AppliedJobIds = appliedJobIds };
    }
}

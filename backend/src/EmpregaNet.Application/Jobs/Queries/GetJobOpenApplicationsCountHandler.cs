using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Jobs.Queries;

/// <summary>
/// Quantas candidaturas da vaga o encerramento vai cancelar.
/// </summary>
public sealed record GetJobOpenApplicationsCountQuery(long JobId) : IRequest<JobOpenApplicationsCountViewModel>;

/// <param name="JobId">Vaga consultada.</param>
/// <param name="OpenApplicationsCount">Candidaturas em <c>Pending</c> ou <c>Processing</c>.</param>
public sealed record JobOpenApplicationsCountViewModel(long JobId, int OpenApplicationsCount);

public sealed class GetJobOpenApplicationsCountHandler
    : IRequestHandler<GetJobOpenApplicationsCountQuery, JobOpenApplicationsCountViewModel>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IJobEmployerAccess _jobEmployerAccess;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly ILogger<GetJobOpenApplicationsCountHandler> _logger;

    public GetJobOpenApplicationsCountHandler(
        IJobRepository jobRepository,
        IJobApplicationRepository jobApplicationRepository,
        IJobEmployerAccess jobEmployerAccess,
        IHttpCurrentUser httpCurrentUser,
        ILogger<GetJobOpenApplicationsCountHandler> logger)
    {
        _jobRepository = jobRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _jobEmployerAccess = jobEmployerAccess;
        _httpCurrentUser = httpCurrentUser;
        _logger = logger;
    }

    public async Task<JobOpenApplicationsCountViewModel> Handle(
        GetJobOpenApplicationsCountQuery request,
        CancellationToken cancellationToken)
    {
        RecruitmentAccess.EnsureRecruitmentStaff(_httpCurrentUser);

        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            throw new NotFoundException($"Vaga com ID '{request.JobId}' não encontrada.");
        }

        // Pertencer ao recrutamento não basta: a fila de candidatos de uma vaga é informação
        // competitiva da empresa que a publicou.
        await _jobEmployerAccess.EnsureCanManageCompanyAsync(job.CompanyId, cancellationToken);

        var count = await _jobApplicationRepository.CountOpenByJobIdAsync(job.Id, cancellationToken);

        _logger.LogInformation(
            "Vaga {JobId} tem {Count} candidatura(s) em aberto.",
            job.Id,
            count);

        return new JobOpenApplicationsCountViewModel(job.Id, count);
    }
}

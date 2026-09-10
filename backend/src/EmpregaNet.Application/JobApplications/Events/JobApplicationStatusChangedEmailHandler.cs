using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Application.JobApplications.Events;

public sealed class JobApplicationStatusChangedEmailHandler : INotificationHandler<JobApplicationStatusChanged>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IJobApplicationEmailService _emailService;
    private readonly AppUrlsOptions _urls;
    private readonly ILogger<JobApplicationStatusChangedEmailHandler> _logger;

    public JobApplicationStatusChangedEmailHandler(
        IJobApplicationRepository jobApplicationRepository,
        IJobApplicationEmailService emailService,
        IOptions<AppUrlsOptions> urls,
        ILogger<JobApplicationStatusChangedEmailHandler> logger)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _emailService = emailService;
        _urls = urls.Value;
        _logger = logger;
    }

    public async Task Handle(JobApplicationStatusChanged notification, CancellationToken cancellationToken)
    {
        try
        {
            var projection = await _jobApplicationRepository.GetNotificationProjectionAsync(
                notification.JobApplicationId,
                cancellationToken);

            if (projection is null)
            {
                _logger.LogWarning(
                    "Notificação de candidatura {ApplicationId} ignorada: registo não encontrado após o commit.",
                    notification.JobApplicationId);
                return;
            }

            if (string.IsNullOrWhiteSpace(projection.CandidateEmail))
            {
                _logger.LogWarning(
                    "Notificação de candidatura {ApplicationId} ignorada: candidato {CandidateId} sem e-mail.",
                    notification.JobApplicationId,
                    projection.CandidateId);
                return;
            }

            var model = new JobApplicationEmailModel(
                CandidateEmail: projection.CandidateEmail,
                CandidateName: projection.CandidateName,
                JobTitle: projection.JobTitle,
                CompanyName: projection.CompanyName,
                StatusDescription: notification.NewStatus.ToDescription(),
                NewStatus: notification.NewStatus,
                OccurredAt: notification.OccurredAt,
                ApplicationsUrl: BuildApplicationsUrl(),
                Reason: notification.Reason);

            await _emailService.SendStatusNotificationAsync(model, cancellationToken);

            _logger.LogInformation(
                "Notificação {Reason} enviada para a candidatura {ApplicationId} (status {Status}).",
                notification.Reason,
                notification.JobApplicationId,
                notification.NewStatus);
        }
        catch (Exception ex)
        {
            // Deliberadamente amplo: qualquer falha aqui - transporte, template, consulta, não pode
            // desfazer nem sinalizar erro numa operação já confirmada.
            _logger.LogError(
                ex,
                "Falha ao notificar a candidatura {ApplicationId} ({Reason}). A operação de negócio está confirmada; o e-mail não foi entregue.",
                notification.JobApplicationId,
                notification.Reason);
        }
    }

    /// <summary>Link para a área autenticada de candidaturas. Sem token: o candidato autentica-se.</summary>
    private string BuildApplicationsUrl()
    {
        var baseUrl = _urls.PublicAppBaseUrl.TrimEnd('/');
        var path = _urls.ApplicationsPath.StartsWith('/') ? _urls.ApplicationsPath : "/" + _urls.ApplicationsPath;
        return $"{baseUrl}{path}";
    }
}

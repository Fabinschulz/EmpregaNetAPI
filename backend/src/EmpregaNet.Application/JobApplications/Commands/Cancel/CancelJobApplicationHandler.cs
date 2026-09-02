using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.JobApplications.Commands;

/// <summary>Desistência declarada pelo próprio candidato.</summary>
public sealed record CancelJobApplicationCommand(long Id) : IRequest<JobApplicationViewModel>, ITransactional;

public sealed class CancelJobApplicationHandler
    : IRequestHandler<CancelJobApplicationCommand, JobApplicationViewModel>
{
    /// <summary>
    /// Mensagem única para candidatura inexistente e para candidatura de outra pessoa.
    /// </summary>
    /// <remarks>
    /// Diferenciar as duas respostas transformaria o endpoint num oráculo de existência de ids: com
    /// um 403 e um 404 na inexistente, bastava percorrer ids para saber quais existem.
    /// </remarks>
    private const string NotFoundMessage = "Candidatura não encontrada.";

    private static readonly string[] NonCandidateRoles = RecruitmentRoleNames.Staff;

    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly ILogger<CancelJobApplicationHandler> _logger;

    public CancelJobApplicationHandler(
        IJobApplicationRepository jobApplicationRepository,
        IHttpCurrentUser httpCurrentUser,
        ILogger<CancelJobApplicationHandler> logger)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _httpCurrentUser = httpCurrentUser;
        _logger = logger;
    }

    public async Task<JobApplicationViewModel> Handle(
        CancelJobApplicationCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelamento da candidatura {ApplicationId} pedido pelo candidato.", request.Id);

        var user = _httpCurrentUser.GetContextUser();
        if (user is null)
        {
            throw ValidationAppException.ForBusinessRule(
                "Usuário autenticado não encontrado no contexto da requisição.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }

        // Cancelar como empresa é a mudança de status já existente. Recusar aqui evita que o ato do
        // recrutamento entre no histórico com a autoria do candidato.
        var userRoles = user.UserToken.GetRoleNames();
        if (userRoles.Any(r => NonCandidateRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
        {
            throw ValidationAppException.ForBusinessRule(
                "Apenas o próprio candidato pode cancelar a sua candidatura.",
                DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
        }

        var application = await _jobApplicationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (application is null || application.IsDeleted || application.UserId != _httpCurrentUser.UserId)
        {
            _logger.LogWarning(
                "Cancelamento recusado: candidatura {ApplicationId} inexistente ou de outro utilizador.",
                request.Id);

            throw new NotFoundException(NotFoundMessage);
        }

        try
        {
            application.CancelByCandidate();
        }
        catch (InvalidOperationException ex)
        {
            throw ValidationAppException.ForBusinessRule(ex.Message, DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        }

        await _jobApplicationRepository.UpdateAsync(application, cancellationToken);

        var canceled = await _jobApplicationRepository.GetProjectionByIdAsync(request.Id, cancellationToken);
        if (canceled is null)
        {
            throw new NotFoundException(NotFoundMessage);
        }

        _logger.LogInformation("Candidatura {ApplicationId} cancelada pelo candidato.", request.Id);

        return canceled.ToViewModel();
    }
}

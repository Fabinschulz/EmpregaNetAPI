using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.JobApplications.Queries;

public sealed class GetJobApplicationByIdHandler : IRequestHandler<GetByIdQuery<JobApplicationViewModel>, JobApplicationViewModel>
{
    private static readonly string[] AllowedRoles = ["Admin", "Recruiter", "Manager"];
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly ILogger<GetJobApplicationByIdHandler> _logger;

    public GetJobApplicationByIdHandler(
        IJobApplicationRepository jobApplicationRepository,
        IHttpCurrentUser httpCurrentUser,
        ILogger<GetJobApplicationByIdHandler> logger)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _httpCurrentUser = httpCurrentUser;
        _logger = logger;
    }

    public async Task<JobApplicationViewModel> Handle(GetByIdQuery<JobApplicationViewModel> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando candidatura por id {ApplicationId}", request.Id);

        var application = await _jobApplicationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (application is null || application.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.Id),
                $"Candidatura com ID '{request.Id}' não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        if (!CanSeeApplication(application.UserId))
        {
            throw new ValidationAppException(
                nameof(_httpCurrentUser.UserId),
                "Você não possui permissão para visualizar esta candidatura.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }

        return application.ToViewModel();
    }

    private bool CanSeeApplication(long ownerUserId)
    {
        var user = _httpCurrentUser.GetContextUser();
        if (user is null)
        {
            return false;
        }

        if (ownerUserId == _httpCurrentUser.UserId)
        {
            return true;
        }

        var userRoles = user.UserToken.Claims
            .Where(c => c.Type.EndsWith("/claims/role", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return userRoles.Any(r => AllowedRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
    }
}

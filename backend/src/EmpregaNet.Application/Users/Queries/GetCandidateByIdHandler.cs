using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Application.Users.Queries;

public sealed record GetCandidateByIdQuery(long Id) : IRequest<CandidateDetailViewModel>;

public sealed class GetCandidateByIdHandler : IRequestHandler<GetCandidateByIdQuery, CandidateDetailViewModel>
{
    private readonly UserManager<User> _userManager;
    private readonly IJobApplicationRepository _applications;
    private readonly TimeProvider _timeProvider;

    public GetCandidateByIdHandler(
        UserManager<User> userManager,
        IJobApplicationRepository applications,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _applications = applications;
        _timeProvider = timeProvider;
    }

    public async Task<CandidateDetailViewModel> Handle(GetCandidateByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user is null || user.IsDeleted || user.UserType != UserTypeEnum.Candidate)
        {
            throw new ValidationAppException(
                nameof(request.Id),
                $"Candidato com ID '{request.Id}' não encontrado.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var applicationsByStatus = await _applications.GetStatusCountsByUserAsync(user.Id, cancellationToken);

        return user.ToCandidateDetail(
            roles.OrderBy(role => role, StringComparer.OrdinalIgnoreCase).ToList(),
            applicationsByStatus,
            _timeProvider.GetUtcNow());
    }
}

using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Application.Users.Queries;

public sealed record GetCandidateByIdQuery(long Id) : IRequest<UserViewModel>;

public sealed class GetCandidateByIdHandler : IRequestHandler<GetCandidateByIdQuery, UserViewModel>
{
    private readonly UserManager<User> _userManager;

    public GetCandidateByIdHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserViewModel> Handle(GetCandidateByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user is null || user.IsDeleted || user.UserType != UserTypeEnum.Candidate)
        {
            throw new ValidationAppException(
                nameof(request.Id),
                $"Candidato com ID '{request.Id}' não encontrado.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        return user.ToViewModel();
    }
}

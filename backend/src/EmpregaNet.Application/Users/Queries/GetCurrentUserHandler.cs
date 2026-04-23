using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Application.Users.Queries;

public sealed record GetCurrentUserQuery() : IRequest<UserViewModel>;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, UserViewModel>
{
    private readonly IHttpCurrentUser _currentUser;
    private readonly UserManager<User> _userManager;

    public GetCurrentUserHandler(IHttpCurrentUser currentUser, UserManager<User> userManager)
    {
        _currentUser = currentUser;
        _userManager = userManager;
    }

    public async Task<UserViewModel> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_currentUser.UserId.ToString());
        if (user is null || user.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(_currentUser.UserId),
                "Usuário não encontrado.",
                DomainErrorEnum.USER_NOT_FOUND);
        }

        return user.ToViewModel();
    }
}

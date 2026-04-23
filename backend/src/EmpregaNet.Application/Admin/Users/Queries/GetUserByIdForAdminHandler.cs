using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Application.Admin.Users.Queries;

public sealed record GetUserByIdForAdminQuery(long Id) : IRequest<UserViewModel>;

public sealed class GetUserByIdForAdminHandler : IRequestHandler<GetUserByIdForAdminQuery, UserViewModel>
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpCurrentUser _httpCurrentUser;

    public GetUserByIdForAdminHandler(UserManager<User> userManager, IHttpCurrentUser httpCurrentUser)
    {
        _userManager = userManager;
        _httpCurrentUser = httpCurrentUser;
    }

    public async Task<UserViewModel> Handle(GetUserByIdForAdminQuery request, CancellationToken cancellationToken)
    {
        AdministradorAccess.EnsureAdministrator(_httpCurrentUser);

        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user is null)
        {
            throw new ValidationAppException(
                nameof(request.Id),
                $"Usuário com ID '{request.Id}' não encontrado.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        return user.ToViewModel();
    }
}

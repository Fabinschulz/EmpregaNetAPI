using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Users.Identity;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Admin.Users.Commands;

/// <summary>
/// Atualização administrativa: apenas o tipo de usuário no domínio.
/// E-mail, login e telefone: <c>PUT /api/users/me</c> (próprio utilizador).
/// </summary>
public sealed record UpdateAdminUserCommand(string UserType);

public sealed class UpdateAdminUserHandler : IRequestHandler<UpdateCommand<UpdateAdminUserCommand, UserViewModel>, UserViewModel>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IOutputCacheManager _cache;
    private readonly ILogger<UpdateAdminUserHandler> _logger;

    public UpdateAdminUserHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IHttpCurrentUser httpCurrentUser,
        IRefreshTokenService refreshTokens,
        IOutputCacheManager cacheService,
        ILogger<UpdateAdminUserHandler> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _httpCurrentUser = httpCurrentUser;
        _refreshTokens = refreshTokens;
        _cache = cacheService;
        _logger = logger;
    }

    public async Task<UserViewModel> Handle(UpdateCommand<UpdateAdminUserCommand, UserViewModel> request, CancellationToken cancellationToken)
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

        if (user.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.Id),
                "Não é possível atualizar um usuário excluído.",
                DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        }

        var entity = request.entity;

        if (!Enum.TryParse<UserTypeEnum>(entity.UserType, ignoreCase: true, out var parsed))
        {
            throw new ValidationAppException(
                nameof(entity.UserType),
                "Tipo de usuário inválido.",
                DomainErrorEnum.INVALID_PARAMS);
        }

        var previousType = user.UserType;
        user.UserType = parsed;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            var msg = update.Errors.FirstOrDefault()?.Description ?? "Falha ao salvar o usuário.";
            throw new ValidationAppException(nameof(request.Id), msg, DomainErrorEnum.RESOURCE_ERROR);
        }

        await UserTypeRoleSync.SyncRolesAsync(user, _userManager, _roleManager, cancellationToken);

        if (previousType != parsed)
        {
            await _refreshTokens.RevokeAllForUserAsync(user.Id, cancellationToken);
        }

        _logger.LogInformation("Usuário {UserId} atualizado por administrador.", user.Id);

        await _cache.InvalidateAdminUsersAsync(user.Id, cancellationToken);

        return user.ToViewModel();
    }
}

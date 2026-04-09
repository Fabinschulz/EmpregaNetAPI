using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Cache;
using EmpregaNet.Application.Common.Exceptions;
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
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IMemoryService _memoryService;
    private readonly ILogger<UpdateAdminUserHandler> _logger;

    public UpdateAdminUserHandler(
        UserManager<User> userManager,
        IHttpCurrentUser httpCurrentUser,
        IMemoryService memoryService,
        ILogger<UpdateAdminUserHandler> logger)
    {
        _userManager = userManager;
        _httpCurrentUser = httpCurrentUser;
        _memoryService = memoryService;
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

        user.UserType = parsed;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            var msg = update.Errors.FirstOrDefault()?.Description ?? "Falha ao salvar o usuário.";
            throw new ValidationAppException(nameof(request.Id), msg, DomainErrorEnum.RESOURCE_ERROR);
        }

        _logger.LogInformation("Usuário {UserId} atualizado por administrador.", user.Id);

        _memoryService.Remove(ApplicationCacheKeys.Users.Me(user.Id));
        await _memoryService.RemoveByPatternAsync(ApplicationCacheKeys.Users.AdminPrefix);
        _memoryService.Remove(ApplicationCacheKeys.Candidates.GetById(user.Id));

        return user.ToViewModel();
    }
}

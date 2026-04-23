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

public sealed class DeleteUserHandler : IRequestHandler<DeleteCommand<UserViewModel>, bool>
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IMemoryService _memoryService;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(
        UserManager<User> userManager,
        IHttpCurrentUser httpCurrentUser,
        IMemoryService memoryService,
        ILogger<DeleteUserHandler> logger)
    {
        _userManager = userManager;
        _httpCurrentUser = httpCurrentUser;
        _memoryService = memoryService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCommand<UserViewModel> request, CancellationToken cancellationToken)
    {
        AdministradorAccess.EnsureAdministrator(_httpCurrentUser);

        if (request.Id == _httpCurrentUser.UserId)
        {
            throw new ValidationAppException(
                nameof(request.Id),
                "Não é possível excluir o próprio usuário administrador autenticado.",
                DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
        }

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
            _logger.LogInformation("Usuário {UserId} já estava excluído (soft delete idempotente).", request.Id);
            return true;
        }

        var now = DateTimeOffset.UtcNow;
        user.IsDeleted = true;
        user.DeletedAt = now;
        user.UpdatedAt = now;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var msg = result.Errors.FirstOrDefault()?.Description ?? "Falha ao excluir o usuário.";
            throw new ValidationAppException(nameof(request.Id), msg, DomainErrorEnum.RESOURCE_ERROR);
        }

        _logger.LogInformation("Usuário {UserId} marcado como excluído (soft delete).", request.Id);

        _memoryService.Remove(ApplicationCacheKeys.Users.Me(user.Id));
        await _memoryService.RemoveByPatternAsync(ApplicationCacheKeys.Users.AdminPrefix);
        _memoryService.Remove(ApplicationCacheKeys.Candidates.GetById(user.Id));

        return true;
    }
}

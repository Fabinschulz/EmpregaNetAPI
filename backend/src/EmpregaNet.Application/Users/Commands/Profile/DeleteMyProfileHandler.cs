using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Users.Commands;

public sealed record DeleteMyProfileCommand : IRequest<bool>;

public sealed class DeleteMyProfileHandler : IRequestHandler<DeleteMyProfileCommand, bool>
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IOutputCacheManager _cache;
    private readonly ILogger<DeleteMyProfileHandler> _logger;

    public DeleteMyProfileHandler(
        UserManager<User> userManager,
        IHttpCurrentUser httpCurrentUser,
        IOutputCacheManager cacheService,
        ILogger<DeleteMyProfileHandler> logger)
    {
        _userManager = userManager;
        _httpCurrentUser = httpCurrentUser;
        _cache = cacheService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteMyProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_httpCurrentUser.UserId.ToString());
        if (user is null)
        {
            throw new ValidationAppException(
                nameof(_httpCurrentUser.UserId),
                "Usuário não encontrado.",
                DomainErrorEnum.USER_NOT_FOUND);
        }

        if (user.IsDeleted)
        {
            _logger.LogInformation("Conta {UserId} já estava excluída (requisição idempotente).", user.Id);
            return true;
        }

        var now = DateTimeOffset.UtcNow;
        user.IsDeleted = true;
        user.DeletedAt = now;
        user.UpdatedAt = now;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var msg = result.Errors.FirstOrDefault()?.Description ?? "Falha ao encerrar a conta.";
            throw new ValidationAppException(nameof(request), msg, DomainErrorEnum.RESOURCE_ERROR);
        }

        _logger.LogInformation("Usuário {UserId} solicitou exclusão da própria conta (delete).", user.Id);

        await _cache.InvalidateAdminUsersAsync(user.Id, cancellationToken);

        return true;
    }
}

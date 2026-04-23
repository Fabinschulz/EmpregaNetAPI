using EmpregaNet.Application.Common.Cache;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Users.Commands;

public sealed record ChangeMyPasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string NewPasswordConfirmation) : IRequest<bool>;

public sealed class ChangeMyPasswordHandler : IRequestHandler<ChangeMyPasswordCommand, bool>
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IMemoryService _memoryService;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly ILogger<ChangeMyPasswordHandler> _logger;

    public ChangeMyPasswordHandler(
        UserManager<User> userManager,
        IHttpCurrentUser httpCurrentUser,
        IMemoryService memoryService,
        IRefreshTokenService refreshTokens,
        ILogger<ChangeMyPasswordHandler> logger)
    {
        _userManager = userManager;
        _httpCurrentUser = httpCurrentUser;
        _memoryService = memoryService;
        _refreshTokens = refreshTokens;
        _logger = logger;
    }

    public async Task<bool> Handle(ChangeMyPasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.NewPasswordConfirmation)
        {
            throw new ValidationAppException(
                nameof(request.NewPasswordConfirmation),
                "A confirmação de senha não confere.",
                DomainErrorEnum.INVALID_PARAMS);
        }

        var user = await _userManager.FindByIdAsync(_httpCurrentUser.UserId.ToString());
        if (user is null || user.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(_httpCurrentUser.UserId),
                "Usuário não encontrado.",
                DomainErrorEnum.USER_NOT_FOUND);
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var msg = result.Errors.FirstOrDefault()?.Description ?? "Não foi possível alterar a senha.";
            throw new ValidationAppException(nameof(request.CurrentPassword), msg, DomainErrorEnum.INVALID_PASSWORD);
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Usuário {UserId} alterou a senha.", user.Id);

        _memoryService.Remove(ApplicationCacheKeys.Users.Me(user.Id));
        await _memoryService.RemoveByPatternAsync(ApplicationCacheKeys.Users.AdminPrefix);

        await _refreshTokens.RevokeAllForUserAsync(user.Id, cancellationToken);

        return true;
    }
}

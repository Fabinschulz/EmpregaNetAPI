using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Users.Commands;

public sealed record ResetPasswordCommand(
    long UserId,
    string Token,
    string NewPassword,
    string NewPasswordConfirmation) : IRequest<bool>;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly UserManager<User> _userManager;
    private readonly IOutputCacheManager _cache;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        UserManager<User> userManager,
        IOutputCacheManager cacheService,
        IRefreshTokenService refreshTokens,
        ILogger<ResetPasswordHandler> logger)
    {
        _userManager = userManager;
        _cache = cacheService;
        _refreshTokens = refreshTokens;
        _logger = logger;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.NewPasswordConfirmation)
        {
            throw new ValidationAppException(
                nameof(request.NewPasswordConfirmation),
                "A confirmação de senha não confere.",
                DomainErrorEnum.INVALID_PARAMS);
        }

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null || user.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.UserId),
                "Não foi possível redefinir a senha. Solicite um novo link.",
                DomainErrorEnum.INVALID_PARAMS);
        }

        var rawToken = Uri.UnescapeDataString(request.Token);
        var result = await _userManager.ResetPasswordAsync(user, rawToken, request.NewPassword);
        if (!result.Succeeded)
        {
            var msg = result.Errors.FirstOrDefault()?.Description ?? "Token inválido ou expirado.";
            throw new ValidationAppException(nameof(request.Token), msg, DomainErrorEnum.INVALID_FORM);
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Senha redefinida para usuário {UserId}.", user.Id);

        await _cache.InvalidateUserMeAsync(user.Id, cancellationToken);
        await _cache.InvalidateAdminUsersAsync(cancellationToken: cancellationToken);

        await _refreshTokens.RevokeAllForUserAsync(user.Id, cancellationToken);

        return true;
    }
}

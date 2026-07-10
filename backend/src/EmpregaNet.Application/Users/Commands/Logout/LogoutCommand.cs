using EmpregaNet.Application.Abstraction;

namespace EmpregaNet.Application.Users.Commands;

/// <summary>
/// Encerra a sessão revogando o refresh token apresentado (cookie httpOnly ou corpo).
/// É idempotente: se nenhum token for informado, apenas confirma o logout (os cookies são limpos na API).
/// </summary>
public sealed record LogoutCommand(string? RefreshToken) : IRequest<bool>;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IRefreshTokenService _refreshTokens;

    public LogoutHandler(IRefreshTokenService refreshTokens)
    {
        _refreshTokens = refreshTokens;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            await _refreshTokens.RevokeAsync(request.RefreshToken, cancellationToken);
        
        return true;
    }
}

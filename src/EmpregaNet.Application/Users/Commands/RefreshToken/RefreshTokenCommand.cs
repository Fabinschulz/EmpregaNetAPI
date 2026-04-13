using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Users.Commands;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<UserLoggedViewModel>;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, UserLoggedViewModel>
{
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IJwtBuilder _jwtBuilder;

    public RefreshTokenHandler(IRefreshTokenService refreshTokens, IJwtBuilder jwtBuilder)
    {
        _refreshTokens = refreshTokens;
        _jwtBuilder = jwtBuilder;
    }

    public async Task<UserLoggedViewModel> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var rotated = await _refreshTokens.RotateAsync(request.RefreshToken, cancellationToken);
        if (rotated is null)
        {
            throw new ValidationAppException(
                nameof(request.RefreshToken),
                "Refresh token inválido ou expirado.",
                DomainErrorEnum.INVALID_FORM);
        }

        var (user, newRefresh) = rotated.Value;
        if (user.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.RefreshToken),
                "Refresh token inválido ou expirado.",
                DomainErrorEnum.INVALID_FORM);
        }

        var vm = await _jwtBuilder.BuildUserTokenAsync(user);
        vm.RefreshToken = newRefresh;
        return vm;
    }
}

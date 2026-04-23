using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Application.Users.Commands;

public sealed record ConfirmEmailCommand(long UserId, string Token) : IRequest<bool>;

public sealed class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, bool>
{
    private readonly UserManager<User> _userManager;

    public ConfirmEmailHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null || user.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.UserId),
                "Não foi possível confirmar o e-mail. O link pode ter expirado ou ser inválido.",
                DomainErrorEnum.INVALID_FORM);
        }

        if (user.EmailConfirmed)
            return true;

        var rawToken = Uri.UnescapeDataString(request.Token);
        var result = await _userManager.ConfirmEmailAsync(user, rawToken);
        if (!result.Succeeded)
        {
            var msg = result.Errors.FirstOrDefault()?.Description ?? "Token inválido ou expirado.";
            throw new ValidationAppException(nameof(request.Token), msg, DomainErrorEnum.INVALID_FORM);
        }

        return true;
    }
}

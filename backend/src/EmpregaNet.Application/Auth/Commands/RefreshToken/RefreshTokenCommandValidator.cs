using FluentValidation;

namespace EmpregaNet.Application.Auth.Commands;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token é obrigatório.");
    }
}

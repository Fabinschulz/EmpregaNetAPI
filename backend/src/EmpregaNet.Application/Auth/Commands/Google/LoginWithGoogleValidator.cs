using FluentValidation;

namespace EmpregaNet.Application.Auth.Commands;

public sealed class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Token de identidade Google (id_token) é obrigatório.");
    }
}

using FluentValidation;

namespace EmpregaNet.Application.Users.Commands;

public sealed class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Token de identidade Google (id_token) é obrigatório.");
    }
}

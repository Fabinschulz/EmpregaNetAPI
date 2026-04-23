using FluentValidation;
using EmpregaNet.Application.Users.Validation;

namespace EmpregaNet.Application.Users.Commands;

public sealed class ChangeMyPasswordCommandValidator : AbstractValidator<ChangeMyPasswordCommand>
{
    public ChangeMyPasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Senha atual é obrigatória.");

        RuleFor(x => x.NewPassword).ApplyNewPassword();

        RuleFor(x => x.NewPasswordConfirmation)
            .NotEmpty().WithMessage("Confirmação de senha é obrigatória.");
    }
}

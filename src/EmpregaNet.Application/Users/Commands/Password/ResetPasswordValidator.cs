using FluentValidation;
using EmpregaNet.Application.Users.Validation;

namespace EmpregaNet.Application.Users.Commands.Password;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Identificador de usuário inválido.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token de redefinição é obrigatório.");

        RuleFor(x => x.NewPassword).ApplyNewPassword();

        RuleFor(x => x.NewPasswordConfirmation)
            .NotEmpty().WithMessage("Confirmação de senha é obrigatória.");
    }
}

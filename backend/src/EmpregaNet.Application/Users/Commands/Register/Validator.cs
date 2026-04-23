using EmpregaNet.Application.Utils;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Application.Users.Validation;
using FluentValidation;

namespace EmpregaNet.Application.Users.Commands;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Nome de usuário é obrigatório.")
            .MaximumLength(100).WithMessage("Nome de usuário deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.Password).ApplyNewPassword();

        RuleFor(x => x.PasswordConfirmation)
            .NotEmpty().WithMessage("Confirmação de senha é obrigatória.");

        RuleFor(x => x.PhoneNumber!)
            .Cascade(CascadeMode.Stop)
            .IsBrazilianCellPhone()
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}

using FluentValidation;

namespace EmpregaNet.Application.Users.Commands;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("O identificador do usuário é inválido.");
        RuleFor(x => x.Token).NotEmpty().WithMessage("Token é obrigatório.");
    }
}

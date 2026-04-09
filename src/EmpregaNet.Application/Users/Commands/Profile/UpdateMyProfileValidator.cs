using FluentValidation;

namespace EmpregaNet.Application.Users.Commands.Profile;

public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x)
            .Must(c => HasAtLeastOneField(c))
            .WithMessage("Informe ao menos um campo para atualizar (email, userName ou phoneNumber).");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .EmailAddress()
                .WithMessage("E-mail inválido.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.UserName), () =>
        {
            RuleFor(x => x.UserName!)
                .MinimumLength(3)
                .MaximumLength(256)
                .WithMessage("O nome de usuário deve ter entre 3 e 256 caracteres.");
        });
    }

    private static bool HasAtLeastOneField(UpdateMyProfileCommand c) =>
        !string.IsNullOrWhiteSpace(c.Email)
        || !string.IsNullOrWhiteSpace(c.UserName)
        || c.PhoneNumber is not null;
}

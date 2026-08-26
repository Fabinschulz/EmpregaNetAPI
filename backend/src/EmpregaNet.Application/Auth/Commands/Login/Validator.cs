using EmpregaNet.Application.Utils.CustomValidation;
using FluentValidation;

namespace EmpregaNet.Application.Auth.Commands;

/// <summary>
/// Só deixa passar um identificador que seja CPF ou e-mail.
/// </summary>
public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Identifier)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o seu CPF ou e-mail.")
            .Must(BeCpfOrEmail).WithMessage("Informe um CPF ou e-mail válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }

    private static bool BeCpfOrEmail(string identifier)
    {
        var value = identifier.Trim();

        return LooksLikeEmail(value)
            ? IsEmailShaped(value)
            : BrazilianDocument.IsValidCpf(value);
    }

    private static bool LooksLikeEmail(string value) => value.Contains('@');

    private static bool IsEmailShaped(string value)
    {
        var at = value.IndexOf('@');

        return at > 0
            && at == value.LastIndexOf('@')
            && at < value.Length - 1
            && !value.Any(char.IsWhiteSpace);
    }
}

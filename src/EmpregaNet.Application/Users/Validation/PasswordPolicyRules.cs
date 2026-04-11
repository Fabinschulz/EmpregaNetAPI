using EmpregaNet.Application.Utils;
using FluentValidation;

namespace EmpregaNet.Application.Users.Validation;

/// <summary>
/// Regras DRY para nova senha (cadastro, troca e reset).
/// </summary>
public static class PasswordPolicyRules
{
    public static IRuleBuilderOptions<T, string> ApplyNewPassword<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.")
            .Matches(Constants.Regex.senha)
            .WithMessage("A senha deve conter maiúscula, minúscula, dígito e caractere especial (@$!%*?&), com pelo menos 8 caracteres.");
    }
}

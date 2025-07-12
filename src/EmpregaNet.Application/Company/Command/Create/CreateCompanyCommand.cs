using EmpregaNet.Application.Common.Command;
using EmpregaNet.Application.Companies.Command;
using FluentValidation;

namespace EmpregaNet.Application.Command.Create;

/// <summary>
/// Validador para o comando de criação de empresa (CreateCommand).
/// Garante a obrigatoriedade dos campos para criação e usa o CompanyDataValidator para validação de formato.
/// </summary>
public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCommand<CompanyCommand>>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(c => c.entity)
            .NotNull()
            .WithMessage("Os dados da empresa para criação não podem ser nulos.");

        RuleFor(c => c.entity)
            .SetValidator(new CompanyDataValidator());
    }
}
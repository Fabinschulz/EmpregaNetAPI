using EmpregaNet.Application.Common.Base;
using FluentValidation;

namespace EmpregaNet.Application.Admin.Company.Commands;

/// <summary>
/// Validador para o comando de criação de empresa (CreateCommand).
/// </summary>
public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCommand<CreateCompanyCommand>>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(c => c.entity)
            .NotNull()
            .WithMessage("Os dados da empresa para criação não podem ser nulos.");

        RuleFor(c => c.entity)
            .SetValidator(new CompanyDataValidator<CreateCompanyCommand>());
    }
}

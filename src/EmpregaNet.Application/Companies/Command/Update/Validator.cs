using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Companies.ViewModel;
using FluentValidation;

namespace EmpregaNet.Application.Companies.Command;

/// <summary>
/// Validador para o comando de atualização de empresa (UpdateCommand).
/// Valida o ID do comando e garante a obrigatoriedade dos campos para atualização.
/// Utiliza o CompanyDataValidator para validação de formato.
/// </summary>
public sealed class UpdateCompanyCommandValidator : AbstractValidator<UpdateCommand<UpdateCompanyCommand, CompanyViewModel>>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("O ID da empresa é obrigatório e deve ser um valor válido para atualização.");

        RuleFor(x => x.entity)
            .NotNull()
            .WithMessage("Os dados da empresa não podem ser nulos.");

        RuleFor(x => x.entity)
            .SetValidator(new CompanyDataValidator<UpdateCompanyCommand>());
    }
}
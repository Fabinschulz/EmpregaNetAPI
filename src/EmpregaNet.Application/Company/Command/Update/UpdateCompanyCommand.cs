using EmpregaNet.Application.Common.Command;
using EmpregaNet.Application.Companies.Command;
using EmpregaNet.Application.Companies.ViewModel;
using FluentValidation;

namespace EmpregaNet.Application.Command.Update;

/// <summary>
/// Validador para o comando de atualização de empresa (UpdateCommand).
/// Valida o ID do comando e garante a obrigatoriedade dos campos para atualização.
/// Utiliza o CompanyDataValidator para validação de formato.
/// </summary>
public sealed class UpdateCompanyCommandValidator : AbstractValidator<UpdateCommand<CompanyCommand, CompanyViewModel>>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("O ID da empresa é obrigatório e deve ser um valor válido para atualização.");

        RuleFor(x => x.entity)
            .NotNull()
            .WithMessage("Os dados da empresa para atualização não podem ser nulos.");

        RuleFor(x => x.entity.Jobs)
             .NotNull().WithMessage("A lista de empregos é obrigatória na atualização.")
             .Must(jobs => jobs != null && jobs.Any()).WithMessage("Deve haver pelo menos um emprego na atualização.");

        RuleFor(x => x.entity)
            .SetValidator(new CompanyDataValidator());
    }
}
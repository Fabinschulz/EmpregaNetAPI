using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Admin.Company.ViewModel;
using FluentValidation;

namespace EmpregaNet.Application.Admin.Company.Commands;

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

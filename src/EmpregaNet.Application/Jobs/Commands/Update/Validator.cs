using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Jobs.ViewModel;
using FluentValidation;

namespace EmpregaNet.Application.Jobs.Commands;

/// <summary>
/// Validador para o comando de atualização de empresa (UpdateCommand).
/// Valida o ID do comando e garante a obrigatoriedade dos campos para atualização.
/// Utiliza o CompanyDataValidator para validação de formato.
/// </summary>
public sealed class UpdateJobCommandValidator : AbstractValidator<UpdateCommand<UpdateJobCommand, JobViewModel>>
{
    public UpdateJobCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("O ID da vaga de emprego é obrigatório e deve ser um valor válido para atualização.");

        RuleFor(x => x.entity)
            .NotNull()
            .WithMessage("Os dados da vaga de emprego não podem ser nulos.");
        RuleFor(x => x.entity)
            .SetValidator(new JobDataValidator<UpdateJobCommand>());
    }
}
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Enums;
using FluentValidation;

namespace EmpregaNet.Application.JobApplications.Commands;

public sealed class ChangeJobApplicationStatusCommandValidator :
    AbstractValidator<UpdateCommand<ChangeJobApplicationStatusCommand, JobApplicationViewModel>>
{
    public ChangeJobApplicationStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id da candidatura inválido.");

        RuleFor(x => x.entity)
            .NotNull()
            .WithMessage("Os dados da candidatura para atualização não podem ser nulos.");

        RuleFor(x => x.entity.Status)
            .NotEmpty()
            .WithMessage("O status é obrigatório.")
            .Must(value => Enum.TryParse<ApplicationStatusEnum>(value, true, out var parsed)
                           && parsed != ApplicationStatusEnum.NaoSelecionado)
            .WithMessage("O status da candidatura é inválido.");
    }
}

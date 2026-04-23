using EmpregaNet.Application.Common.Base;
using FluentValidation;

namespace EmpregaNet.Application.JobApplications.Commands;

public sealed class ApplyToJobCommandValidator : AbstractValidator<CreateCommand<ApplyToJobCommand>>
{
    public ApplyToJobCommandValidator()
    {
        RuleFor(x => x.entity)
            .NotNull()
            .WithMessage("Os dados da candidatura não podem ser nulos.");

        RuleFor(x => x.entity.JobId)
            .GreaterThan(0)
            .WithMessage("Id da vaga inválido.");
    }
}

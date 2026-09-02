using FluentValidation;

namespace EmpregaNet.Application.JobApplications.Commands;

public sealed class CancelJobApplicationCommandValidator : AbstractValidator<CancelJobApplicationCommand>
{
    public CancelJobApplicationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id da candidatura inválido.");
    }
}

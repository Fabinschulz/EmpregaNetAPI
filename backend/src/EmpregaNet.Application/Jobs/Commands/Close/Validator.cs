using FluentValidation;

namespace EmpregaNet.Application.Jobs.Commands;

public sealed class CloseJobCommandValidator : AbstractValidator<CloseJobCommand>
{
    public CloseJobCommandValidator()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0)
            .WithMessage("Id da vaga inválido.");
    }
}

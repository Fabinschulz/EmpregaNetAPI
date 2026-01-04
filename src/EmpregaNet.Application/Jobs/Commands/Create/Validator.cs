using EmpregaNet.Application.Common.Base;
using FluentValidation;

namespace EmpregaNet.Application.Jobs.Commands;

/// <summary>
/// Validador para o comando de criação de vaga de emprego (CreateCommand).
/// Garante a obrigatoriedade dos campos para criação e usa o JobDataValidator para validação de formato.
/// </summary>
public sealed class CreateJobCommandValidator : AbstractValidator<CreateCommand<CreateJobCommand>>
{
    public CreateJobCommandValidator()
    {
        RuleFor(c => c.entity)
            .NotNull()
            .WithMessage("Os dados da vaga de emprego para criação não podem ser nulos.");

        RuleFor(c => c.entity)
            .SetValidator(new JobDataValidator<CreateJobCommand>());
    }
}
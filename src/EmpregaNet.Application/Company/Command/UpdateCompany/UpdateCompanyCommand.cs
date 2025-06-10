using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using FluentValidation;

namespace EmpregaNet.Application.Companies.Command;

public sealed record UpdateCompanyCommand(
     List<string> Jobs,
     string? CompanyName,
     string? Email,
     string? Phone,
     Address? Address,
     TypeOfActivityEnum? TypeOfActivity
);


public sealed class UpdateCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCommandValidator()
    {
        RuleFor(x => x.Jobs)
            .NotEmpty().WithMessage("A lista de empregos não pode estar vazia.")
            .Must(jobs => jobs.Count > 0).WithMessage("A lista de empregos deve conter pelo menos um emprego.");
    }
}
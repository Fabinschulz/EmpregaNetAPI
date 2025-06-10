using System.ComponentModel.DataAnnotations;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using FluentValidation;

namespace EmpregaNet.Application.Companies.Command;

public sealed record CreateCompanyCommand(
   string CompanyName,
   string RegistrationNumber,
   string Email,
   string Phone,
   Address Address,
   [EnumDataType(typeof(TypeOfActivityEnum))]
   TypeOfActivityEnum TypeOfActivity,
   ICollection<Job>? Jobs
);

public sealed class CreateCompanyDtoValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyDtoValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("O nome da empresa é obrigatório.")
            .MinimumLength(3).WithMessage("O nome da empresa deve ter no mínimo 3 caracteres.")
            .MaximumLength(100).WithMessage("O nome da empresa deve ter no máximo 100 caracteres.");

        RuleFor(x => x.RegistrationNumber)
            .NotEmpty().WithMessage("O CNPJ é obrigatório.")
            .Matches(@"^\d{2}\.\d{3}\.\d{3}\/\d{4}\-\d{2}$").WithMessage("CNPJ inválido.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("O telefone é obrigatório.")
            .Matches(@"^\(?\d{2}\)?[\s-]?\d{4,5}-?\d{4}$").WithMessage("Telefone inválido.");

        RuleFor(x => x.TypeOfActivity)
            .IsInEnum().WithMessage("Tipo de atividade inválido.");

        RuleFor(x => x.Address)
            .NotNull().WithMessage("O endereço é obrigatório.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Address.Street)
                    .NotEmpty().WithMessage("A rua é obrigatória.")
                    .MaximumLength(200).WithMessage("A rua deve ter no máximo 200 caracteres.");

                RuleFor(x => x.Address.City)
                    .NotEmpty().WithMessage("A cidade é obrigatória.")
                    .MaximumLength(100).WithMessage("A cidade deve ter no máximo 100 caracteres.");

                RuleFor(x => x.Address.State)
                    .IsInEnum().WithMessage("Estado inválido.");

                RuleFor(x => x.Address.ZipCode)
                    .NotEmpty().WithMessage("O CEP é obrigatório.")
                    .Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP inválido.");
            });
    }
}
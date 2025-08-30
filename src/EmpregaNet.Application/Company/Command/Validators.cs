using FluentValidation;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Application.Common.Base;
using System.ComponentModel.DataAnnotations;
using EmpregaNet.Application.Jobs.Commands;

namespace EmpregaNet.Application.Companies.Command;

public sealed record CompanyCommand(
    string CompanyName,
    string RegistrationNumber,
    string Email,
    string Phone,
    [EnumDataType(typeof(TypeOfActivityEnum))]
    TypeOfActivityEnum TypeOfActivity,
    Address Address,
    ICollection<JobCommand>? Jobs = null
);

/// <summary>
/// Validador específico para o DTO CompanyCommand, contendo as regras de validação
/// de formato e tamanho para os dados de uma empresa.
/// Este validador é reutilizado por validadores de comando específicos (Create, Update).
/// </summary>
public sealed class CompanyDataValidator : AbstractValidator<CompanyCommand>
{
    public CompanyDataValidator()
    {

        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .WithMessage("O nome da empresa é obrigatório.")
            .MinimumLength(3).WithMessage("O nome da empresa deve ter no mínimo 3 caracteres.")
            .MaximumLength(100).WithMessage("O nome da empresa deve ter no máximo 100 caracteres.");

        RuleFor(x => x.RegistrationNumber)
            .NotEmpty()
            .WithMessage("O número de registro da empresa é obrigatório.")
            .Matches(@"^\d{14}$")
            .WithMessage("CNPJ inválido. Deve conter exatamente 14 dígitos.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("O e-mail da empresa é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("O telefone da empresa é obrigatório.")
            .Matches(@"^\d{10,11}$")
            .WithMessage("Telefone inválido. Deve conter entre 10 e 11 dígitos numéricos.");


        RuleFor(x => x.Address)
            .NotNull()
            .WithMessage("O endereço da empresa é obrigatório.")
            .SetValidator(new AddressValidator() as IValidator<Address?>)
            .When(x => x.Address is not null);

        RuleFor(x => x.TypeOfActivity)
            .IsInEnum()
            .WithMessage("Tipo de atividade inválido.");
    }
}
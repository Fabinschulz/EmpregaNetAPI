using EmpregaNet.Domain.Entities;
using FluentValidation;

namespace EmpregaNet.Application.Common.Base;

/// <summary>
/// Validador para a entidade Address, contendo as regras de validação para um endereço.
/// </summary>
public sealed class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(a => a.Street)
            .NotEmpty().WithMessage("A rua é obrigatória.")
            .MaximumLength(200).WithMessage("A rua deve ter no máximo 200 caracteres.");

        RuleFor(a => a.City)
            .NotEmpty().WithMessage("A cidade é obrigatória.")
            .MaximumLength(100).WithMessage("A cidade deve ter no máximo 100 caracteres.");

        RuleFor(a => a.State)
            .IsInEnum().WithMessage("Estado inválido.");

        RuleFor(a => a.ZipCode)
            .NotEmpty().WithMessage("O CEP é obrigatório.")
            .Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP inválido.");
    }
}
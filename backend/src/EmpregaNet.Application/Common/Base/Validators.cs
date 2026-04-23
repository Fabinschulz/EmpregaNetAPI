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

public interface IPaginatedQuery
{
    int Page { get; }
    int Size { get; }
    string? OrderBy { get; }
}

public abstract class BasePaginatedQueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : class, IPaginatedQuery
{
    protected BasePaginatedQueryValidator()
    {
        RuleFor(x => x.Page)
            .NotEmpty().WithMessage("Page é obrigatório")
            .GreaterThanOrEqualTo(1).WithMessage("A página precisa ser maior ou igual a 1");

        RuleFor(x => x.Size)
            .NotEmpty().WithMessage("Size é obrigatório")
            .GreaterThanOrEqualTo(100).WithMessage("Size precisa ser maior ou igual a 100");

        RuleFor(x => x.OrderBy)
            .MaximumLength(50).WithMessage("Ordenação deve ter no máximo 50 caracteres.");
    }
}
using EmpregaNet.Domain.Enums;
using FluentValidation;

namespace EmpregaNet.Application.Jobs.Commands;

public interface IJobCommand
{
    long CompanyId { get; }
    string Title { get; }
    string Description { get; }
    decimal Salary { get; }
    string JobType { get; }
};


/// <summary>
/// Validador específico para o DTO CompanyCommand, contendo as regras de validação
/// de formato e tamanho para os dados de uma empresa.
/// Este validador é reutilizado por validadores de comando específicos (Create, Update).
/// </summary>
public sealed class JobDataValidator<T> : AbstractValidator<T> where T : IJobCommand
{
    public JobDataValidator()
    {

        RuleFor(x => x.JobType)
            .NotEmpty()
            .WithMessage("O tipo de trabalho é obrigatório.")
            .Must(value => Enum.TryParse<JobTypeEnum>(value, true, out var parsed)
                        && parsed != JobTypeEnum.NaoSelecionado)
            .WithMessage("O tipo de trabalho fornecido é inválido.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("O título do trabalho é obrigatório.")
            .MaximumLength(100)
            .WithMessage("O título do trabalho deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("A descrição do trabalho é obrigatória.");
        
        RuleFor(x => x.Salary)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O salário deve ser maior ou igual a zero.");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0)
            .WithMessage("Id da empresa inválido.");
    }
}
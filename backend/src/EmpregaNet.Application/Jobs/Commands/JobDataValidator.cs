using EmpregaNet.Domain.Enums;
using FluentValidation;

namespace EmpregaNet.Application.Jobs.Commands;

/// <summary>
/// Regras compartilhadas de dados da vaga (Create/Update).
/// </summary>
public sealed class JobDataValidator<T> : AbstractValidator<T> where T : IJobCommand
{
    public JobDataValidator()
    {
        RuleFor(x => x.JobType)
            .NotEmpty()
            .WithMessage("O tipo de contratação é obrigatório.")
            .Must(value => IsSelectedEnum(value, JobTypeEnum.NaoSelecionado))
            .WithMessage("O tipo de contratação fornecido é inválido.");

        RuleFor(x => x.WorkModel)
            .NotEmpty()
            .WithMessage("A modalidade de trabalho é obrigatória.")
            .Must(value => IsSelectedEnum(value, WorkModelEnum.NaoSelecionado))
            .WithMessage("A modalidade de trabalho fornecida é inválida.");

        RuleFor(x => x.WorkShift)
            .NotEmpty()
            .WithMessage("O turno é obrigatório.")
            .Must(value => IsSelectedEnum(value, WorkShiftEnum.NaoSelecionado))
            .WithMessage("O turno fornecido é inválido.");

        RuleFor(x => x.ExperienceLevel)
            .NotEmpty()
            .WithMessage("A experiência exigida é obrigatória.")
            .Must(value => IsSelectedEnum(value, ExperienceLevelEnum.NaoSelecionado))
            .WithMessage("A experiência exigida é inválida.");

        RuleFor(x => x.Area)
            .NotEmpty()
            .WithMessage("A área é obrigatória.")
            .Must(value => IsSelectedEnum(value, JobAreaEnum.NaoSelecionado))
            .WithMessage("A área fornecida é inválida.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("O título da vaga é obrigatório.")
            .MaximumLength(100)
            .WithMessage("O título da vaga deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Summary)
            .MaximumLength(280)
            .WithMessage("O resumo deve ter no máximo 280 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("A descrição da vaga é obrigatória.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("A cidade da vaga é obrigatória.")
            .MaximumLength(100)
            .WithMessage("A cidade deve ter no máximo 100 caracteres.");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("O estado da vaga é obrigatório.")
            .Must(value => IsSelectedEnum(value, UF.NaoSelecionado))
            .WithMessage("Estado inválido.");

        // Salário divulgado exige ao menos um limite: marcar como divulgado sem informar valor
        // colocaria a vaga em filtros de faixa salarial sem ter faixa nenhuma.
        RuleFor(x => x.SalaryMin)
            .Must((command, _) => !command.SalaryDisclosed || command.SalaryMin.HasValue || command.SalaryMax.HasValue)
            .WithMessage("Informe ao menos o piso ou o teto salarial, ou marque o salário como a combinar.");

        RuleFor(x => x.SalaryMin)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SalaryMin.HasValue)
            .WithMessage("O salário deve ser maior ou igual a zero.");

        RuleFor(x => x.SalaryMax)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SalaryMax.HasValue)
            .WithMessage("O salário deve ser maior ou igual a zero.");

        RuleFor(x => x.SalaryMax)
            .GreaterThanOrEqualTo(x => x.SalaryMin!.Value)
            .When(x => x.SalaryMin.HasValue && x.SalaryMax.HasValue)
            .WithMessage("O teto salarial não pode ser menor que o piso.");

        RuleFor(x => x.Requirements)
            .Must(items => items is null || items.Count <= JobVocabulary.MaxItemsPerJob)
            .WithMessage($"Selecione no máximo {JobVocabulary.MaxItemsPerJob} requisitos.")
            .Must(items => items is null || items.All(JobVocabulary.IsKnownRequirement))
            .WithMessage("Há requisito fora da lista suportada.");

        RuleFor(x => x.Benefits)
            .Must(items => items is null || items.Count <= JobVocabulary.MaxItemsPerJob)
            .WithMessage($"Selecione no máximo {JobVocabulary.MaxItemsPerJob} benefícios.")
            .Must(items => items is null || items.All(JobVocabulary.IsKnownBenefit))
            .WithMessage("Há benefício fora da lista suportada.");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0)
            .WithMessage("Id da empresa inválido.");
    }

    /// <summary>
    /// Aceita o nome do enum e rejeita o membro neutro: <c>NaoSelecionado</c> analisa com sucesso,
    /// mas significa "não escolhido", deixá-lo passar gravaria a vaga sem o dado.
    /// </summary>
    private static bool IsSelectedEnum<TEnum>(string? value, TEnum neutral) where TEnum : struct, Enum
        => Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)
           && Enum.IsDefined(parsed)
           && !parsed.Equals(neutral);
}

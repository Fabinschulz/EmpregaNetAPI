using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Jobs.Queries;
using EmpregaNet.Application.Jobs.ViewModel;
using FluentValidation;

public sealed class JobViewModelGetAllValidator : BasePaginatedQueryValidator<GetAllQuery<JobViewModel>>
{
    public JobViewModelGetAllValidator() : base()
    {
    }
}

public sealed class GetJobFeedInteractionsValidator : AbstractValidator<GetJobFeedInteractionsQuery>
{
    /// <summary>Teto alinhado a algumas páginas de feed; acima disso é uso indevido do endpoint.</summary>
    public const int MaxJobIds = 100;

    public GetJobFeedInteractionsValidator()
    {
        RuleFor(x => x.JobIds)
            .NotNull()
            .WithMessage("Informe as vagas a consultar.");

        RuleFor(x => x.JobIds)
            .Must(ids => ids is null || ids.Count <= MaxJobIds)
            .WithMessage($"Consulte no máximo {MaxJobIds} vagas por vez.");
    }
}

public sealed class GetJobsFeedValidator : BasePaginatedQueryValidator<GetJobsFeedQuery>
{
    public const int MaxPageSize = 50;
    public const int MaxFilterValues = 20;

    public GetJobsFeedValidator() : base()
    {
        RuleFor(x => x.Size)
            .LessThanOrEqualTo(MaxPageSize)
            .WithMessage($"O tamanho de página do feed não pode exceder {MaxPageSize}.");

        RuleFor(x => x.Search)
            .MaximumLength(120)
            .WithMessage("A busca não pode exceder 120 caracteres.");

        RuleForEachCollection(x => x.Cities, "cidades");
        RuleForEachCollection(x => x.States, "estados");
        RuleForEachCollection(x => x.WorkModels, "modalidades");
        RuleForEachCollection(x => x.WorkShifts, "turnos");
        RuleForEachCollection(x => x.JobTypes, "tipos de contratação");
        RuleForEachCollection(x => x.ExperienceLevels, "faixas de experiência");
        RuleForEachCollection(x => x.Areas, "áreas");
        RuleForEachCollection(x => x.Requirements, "requisitos");
        RuleForEachCollection(x => x.Benefits, "benefícios");
        RuleForEachCollection(x => x.CompanyIds, "empresas");

        RuleFor(x => x.SalaryMin)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SalaryMin.HasValue)
            .WithMessage("O piso salarial não pode ser negativo.");

        RuleFor(x => x.SalaryMax)
            .GreaterThanOrEqualTo(x => x.SalaryMin!.Value)
            .When(x => x.SalaryMin.HasValue && x.SalaryMax.HasValue)
            .WithMessage("O teto salarial não pode ser menor que o piso.");
    }

    private void RuleForEachCollection<TItem>(
        System.Linq.Expressions.Expression<Func<GetJobsFeedQuery, IReadOnlyCollection<TItem>?>> selector,
        string label)
    {
        RuleFor(selector)
            .Must(items => items is null || items.Count <= MaxFilterValues)
            .WithMessage($"Selecione no máximo {MaxFilterValues} {label}.");
    }
}

using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Domain.Enums;
using FluentValidation;

namespace EmpregaNet.Application.Dashboard.Queries;

/// <summary>
/// Regras comuns aos recortes do dashboard.
/// </summary>
public sealed class DashboardFilterInputValidator : AbstractValidator<DashboardFilterInput>
{
    /// <summary>
    /// Valores por filtro de múltipla escolha.
    /// </summary>
    public const int MaxFilterValues = 20;

    public DashboardFilterInputValidator()
    {
        RuleFor(x => x.Period)
            .IsInEnum()
            .WithMessage("Período inválido.");

        RuleFor(x => x.From)
            .NotNull()
            .When(x => x.Period == DashboardPeriodEnum.Custom)
            .WithMessage("Informe a data inicial do período personalizado.");

        RuleFor(x => x.To)
            .NotNull()
            .When(x => x.Period == DashboardPeriodEnum.Custom)
            .WithMessage("Informe a data final do período personalizado.");

        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From!.Value)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("A data final não pode ser anterior à data inicial.");

        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue
                       || x.To.Value.DayNumber - x.From.Value.DayNumber + 1 <= DashboardPeriodResolver.MaxCustomRangeDays)
            .WithMessage($"O período personalizado não pode exceder {DashboardPeriodResolver.MaxCustomRangeDays} dias.")
            .OverridePropertyName("to");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0)
            .When(x => x.CompanyId.HasValue)
            .WithMessage("Empresa inválida.");

        RuleFor(x => x.States)
            .Must(states => states is null || states.Count <= MaxFilterValues)
            .WithMessage($"Selecione no máximo {MaxFilterValues} estados.");

        RuleFor(x => x.Areas)
            .Must(areas => areas is null || areas.Count <= MaxFilterValues)
            .WithMessage($"Selecione no máximo {MaxFilterValues} áreas.");

        RuleFor(x => x.ApplicationStatus)
            .IsInEnum()
            .When(x => x.ApplicationStatus.HasValue)
            .WithMessage("Status de candidatura inválido.");
    }
}

public sealed class GetDashboardOverviewValidator : AbstractValidator<GetDashboardOverviewQuery>
{
    public GetDashboardOverviewValidator()
    {
        RuleFor(x => x.Filter).NotNull().SetValidator(new DashboardFilterInputValidator());
    }
}

public sealed class GetDashboardTrendsValidator : AbstractValidator<GetDashboardTrendsQuery>
{
    public GetDashboardTrendsValidator()
    {
        RuleFor(x => x.Filter).NotNull().SetValidator(new DashboardFilterInputValidator());

        RuleFor(x => x.Granularity)
            .IsInEnum()
            .When(x => x.Granularity.HasValue)
            .WithMessage("Granularidade inválida.");
    }
}

public sealed class GetDashboardDistributionValidator : AbstractValidator<GetDashboardDistributionQuery>
{
    public GetDashboardDistributionValidator()
    {
        RuleFor(x => x.Filter).NotNull().SetValidator(new DashboardFilterInputValidator());
    }
}

public sealed class GetDashboardJobsValidator : AbstractValidator<GetDashboardJobsQuery>
{
    /// <summary>
    /// Teto de linhas do ranking.
    /// </summary>
    public const int MaxLimit = 50;

    public GetDashboardJobsValidator()
    {
        RuleFor(x => x.Filter).NotNull().SetValidator(new DashboardFilterInputValidator());

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, MaxLimit)
            .WithMessage($"O ranking de vagas deve ter entre 1 e {MaxLimit} linhas.");

        RuleFor(x => x.Ranking)
            .IsInEnum()
            .WithMessage("Critério de ranking inválido.");
    }
}

public sealed class GetDashboardInsightsValidator : AbstractValidator<GetDashboardInsightsQuery>
{
    public GetDashboardInsightsValidator()
    {
        RuleFor(x => x.Filter).NotNull().SetValidator(new DashboardFilterInputValidator());
    }
}


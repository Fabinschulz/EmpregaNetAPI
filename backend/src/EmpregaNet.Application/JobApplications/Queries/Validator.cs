using EmpregaNet.Application.Common.Base;
using EmpregaNet.Domain.Enums;
using FluentValidation;

namespace EmpregaNet.Application.JobApplications.Queries;

/// <summary>Teto do termo de busca nas listagens de candidaturas, alinhado ao do feed de vagas.</summary>
internal static class JobApplicationsQueryLimits
{
    public const int MaxSearchLength = 120;
}

/// <remarks>
/// Sem regra de <c>Status</c>: quem valida é o <see cref="ApplicationStatusParser"/>, no handler, que é
/// a fonte única e responde com <c>INVALID_QUERY_FILTER</c>. Uma regra aqui recusaria antes, com outro
/// código de erro.
/// </remarks>
public sealed class GetAllJobApplicationsValidator : BasePaginatedQueryValidator<GetAllJobApplicationsQuery>
{
    public GetAllJobApplicationsValidator() : base()
    {
        RuleFor(x => x.Search)
            .MaximumLength(JobApplicationsQueryLimits.MaxSearchLength)
            .WithMessage($"A busca não pode exceder {JobApplicationsQueryLimits.MaxSearchLength} caracteres.");
    }
}

public sealed class GetMyJobApplicationsQueryValidator : BasePaginatedQueryValidator<GetMyJobApplicationsQuery>
{
    public GetMyJobApplicationsQueryValidator() : base()
    {
        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) ||
                           (Enum.TryParse<ApplicationStatusEnum>(value, true, out var parsed) &&
                            parsed != ApplicationStatusEnum.NaoSelecionado))
            .WithMessage("Status de candidatura inválido.");
    }
}

/// <summary>
/// Herda as regras de paginação da base, como os validadores irmãos. Reescrevê-las aqui foi o que
/// permitiu a divergência: o limite de <c>Size</c> ficou como mínimo de 100, e a consulta rejeitava
/// qualquer página menor do que isso.
/// </summary>
/// <remarks>
/// Sem regra de <c>Status</c>, pelo mesmo motivo de <see cref="GetAllJobApplicationsValidator"/>: o
/// <see cref="ApplicationStatusParser"/> é a fonte única.
/// </remarks>
public sealed class GetJobApplicationsByJobIdQueryValidator
    : BasePaginatedQueryValidator<GetJobApplicationsByJobIdQuery>
{
    public GetJobApplicationsByJobIdQueryValidator() : base()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0)
            .WithMessage("Id da vaga inválido.");

        RuleFor(x => x.Search)
            .MaximumLength(JobApplicationsQueryLimits.MaxSearchLength)
            .WithMessage($"A busca não pode exceder {JobApplicationsQueryLimits.MaxSearchLength} caracteres.");
    }
}

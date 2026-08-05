using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Enums;
using FluentValidation;

namespace EmpregaNet.Application.JobApplications.Queries;

public sealed class GetAllJobApplicationsValidator : BasePaginatedQueryValidator<GetAllQuery<JobApplicationViewModel>>
{
    public GetAllJobApplicationsValidator() : base()
    {
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
public sealed class GetJobApplicationsByJobIdQueryValidator
    : BasePaginatedQueryValidator<GetJobApplicationsByJobIdQuery>
{
    public GetJobApplicationsByJobIdQueryValidator() : base()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0)
            .WithMessage("Id da vaga inválido.");

        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) ||
                           (Enum.TryParse<ApplicationStatusEnum>(value, true, out var parsed) &&
                            parsed != ApplicationStatusEnum.NaoSelecionado))
            .WithMessage("Status de candidatura inválido.");
    }
}

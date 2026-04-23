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

public sealed class GetJobApplicationsByJobIdQueryValidator : AbstractValidator<GetJobApplicationsByJobIdQuery>
{
    public GetJobApplicationsByJobIdQueryValidator()
    {
        RuleFor(x => x.Page)
            .NotEmpty().WithMessage("Page é obrigatório")
            .GreaterThanOrEqualTo(1).WithMessage("A página precisa ser maior ou igual a 1");

        RuleFor(x => x.Size)
            .NotEmpty().WithMessage("Size é obrigatório")
            .GreaterThanOrEqualTo(100).WithMessage("Size precisa ser maior ou igual a 100");

        RuleFor(x => x.OrderBy)
            .MaximumLength(50).WithMessage("Ordenação deve ter no máximo 50 caracteres.");

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

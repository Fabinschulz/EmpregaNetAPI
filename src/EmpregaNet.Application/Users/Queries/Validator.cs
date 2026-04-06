using EmpregaNet.Application.Common.Base;
using FluentValidation;

namespace EmpregaNet.Application.Users.Queries;

public sealed class GetAllCandidatesQueryValidator : BasePaginatedQueryValidator<GetAllCandidatesQuery>
{
    public GetAllCandidatesQueryValidator() : base()
    {
    }
}

public sealed class GetCandidateByIdQueryValidator : AbstractValidator<GetCandidateByIdQuery>
{
    public GetCandidateByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id do candidato inválido.");
    }
}

using FluentValidation;

namespace EmpregaNet.Application.Admin.Users.Queries;

public sealed class GetUserByIdForAdminQueryValidator : AbstractValidator<GetUserByIdForAdminQuery>
{
    public GetUserByIdForAdminQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id do usuário inválido.");
    }
}

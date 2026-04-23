using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Enums;
using FluentValidation;

namespace EmpregaNet.Application.Admin.Users.Commands;

public sealed class UpdateAdminUserCommandValidator : AbstractValidator<UpdateCommand<UpdateAdminUserCommand, UserViewModel>>
{
    public UpdateAdminUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("O ID do usuário é obrigatório.");

        RuleFor(x => x.entity)
            .NotNull()
            .WithMessage("O corpo da requisição não pode ser nulo.");

        RuleFor(x => x.entity!.UserType)
            .NotEmpty()
            .WithMessage("O tipo de usuário é obrigatório.")
            .Must(t => Enum.TryParse<UserTypeEnum>(t, ignoreCase: true, out var v) && v != UserTypeEnum.NaoSelecionado)
            .WithMessage("Tipo de usuário inválido. Utilize um valor do enum (ex.: Candidate, Recruiter, Admin).");
    }
}

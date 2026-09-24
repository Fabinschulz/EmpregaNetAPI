using EmpregaNet.Application.Admin.Users.Commands;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Users.ViewModel;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.Admin.Users;

/// <summary>
/// <c>PUT /api/admin/{id}</c>: <c>UserType</c> só aceita o nome de um membro declarado do
/// <c>UserTypeEnum</c>, via <c>EnumNameParser</c>.
/// </summary>
public sealed class UpdateAdminUserCommandValidatorTests
{
    private readonly UpdateAdminUserCommandValidator _sut = new();

    private static UpdateCommand<UpdateAdminUserCommand, UserViewModel> Command(string userType) =>
        new(Id: 1, entity: new UpdateAdminUserCommand(userType));

    [Theory]
    [InlineData("Recruiter")]
    [InlineData("recruiter")]
    [InlineData("RECRUITER")]
    [InlineData("Admin")]
    [InlineData("Candidate")]
    [InlineData("Manager")]
    public void UserType_NomeDeclaradoEmQualquerCaixa_DeveSerAceito(string userType)
    {
        var result = _sut.Validate(Command(userType));

        result.IsValid.Should().BeTrue(because: string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    // Regressão: "99" e "1" passam no Enum.TryParse (aceita número); "Candidate,Recruiter" faria OU bit a
    // bit (= Admin); "Administrador" é o rótulo pt-BR, não o nome do enum.
    [Theory]
    [InlineData("99")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("Candidate,Recruiter")]
    [InlineData("NaoSelecionado")]
    [InlineData("naoselecionado")]
    [InlineData("Administrador")]
    [InlineData("Inexistente")]
    public void UserType_Invalido_DeveSerRecusado(string userType)
    {
        var result = _sut.Validate(Command(userType));

        result.IsValid.Should().BeFalse();
    }
}

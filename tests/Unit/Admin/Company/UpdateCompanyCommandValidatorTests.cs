using EmpregaNet.Application.Admin.Company.Commands;
using EmpregaNet.Application.Admin.Company.ViewModel;
using EmpregaNet.Application.Common.Base;
using FluentAssertions;
using FluentValidation;

namespace EmpregaNet.Tests.Unit.Application.Admin.CompanyHandlers;

public sealed class UpdateCompanyCommandValidatorTests
{
    private readonly IValidator<UpdateCommand<UpdateCompanyCommand, CompanyViewModel>> _sut =
        new UpdateCompanyCommandValidator();

    [Fact]
    public void Validate_IdValidoEComandoValido_DevePassar()
    {
        var cmd = new UpdateCommand<UpdateCompanyCommand, CompanyViewModel>(1, CompanyTestData.ValidUpdateCommand());

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_IdZero_DeveFalhar()
    {
        var cmd = new UpdateCommand<UpdateCompanyCommand, CompanyViewModel>(0, CompanyTestData.ValidUpdateCommand());

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCommand<UpdateCompanyCommand, CompanyViewModel>.Id));
    }

    [Fact]
    public void Validate_EntityNulo_DeveFalhar()
    {
        var cmd = new UpdateCommand<UpdateCompanyCommand, CompanyViewModel>(1, null!);

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.StartsWith("entity"));
    }

    [Fact]
    public void Validate_NomeEmpresaCurto_DeveFalhar()
    {
        var entity = CompanyTestData.ValidUpdateCommand() with { CompanyName = "AB" };
        var cmd = new UpdateCommand<UpdateCompanyCommand, CompanyViewModel>(1, entity);

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCompanyCommand.CompanyName));
    }
}

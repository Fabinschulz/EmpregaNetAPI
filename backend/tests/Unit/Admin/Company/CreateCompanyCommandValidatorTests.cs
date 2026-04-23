using EmpregaNet.Application.Admin.Company.Commands;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Domain.Enums;
using FluentAssertions;
using FluentValidation;

namespace EmpregaNet.Tests.Unit.Application.Admin.CompanyHandlers;

/// <summary>
/// Regras de entrada da criação de empresa (CNPJ 14 dígitos, tipo de atividade, endereço).
/// </summary>
public sealed class CreateCompanyCommandValidatorTests
{
    private readonly IValidator<CreateCommand<CreateCompanyCommand>> _sut = new CreateCompanyCommandValidator();

    [Fact]
    public void Validate_ComandoValido_DevePassar()
    {
        var cmd = new CreateCommand<CreateCompanyCommand>(CompanyTestData.ValidCreateCommand());

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EntityNulo_DeveFalhar()
    {
        var cmd = new CreateCommand<CreateCompanyCommand>(null!);

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.StartsWith("entity"));
    }

    [Fact]
    public void Validate_CnpjComMenosDe14Digitos_DeveFalhar()
    {
        var entity = CompanyTestData.ValidCreateCommand() with { Cnpj = "123456789012" };
        var cmd = new CreateCommand<CreateCompanyCommand>(entity);

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCompanyCommand.Cnpj));
    }

    [Fact]
    public void Validate_TipoDeAtividadeNaoSelecionado_DeveFalhar()
    {
        var entity = CompanyTestData.ValidCreateCommand() with { TypeOfActivity = nameof(TypeOfActivityEnum.NaoSelecionado) };
        var cmd = new CreateCommand<CreateCompanyCommand>(entity);

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCompanyCommand.TypeOfActivity));
    }

    [Fact]
    public void Validate_TelefoneComCaracteresNaoNumericos_DeveFalhar()
    {
        var entity = CompanyTestData.ValidCreateCommand() with { Phone = "(11) 3333-4444" };
        var cmd = new CreateCommand<CreateCompanyCommand>(entity);

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCompanyCommand.Phone));
    }
}

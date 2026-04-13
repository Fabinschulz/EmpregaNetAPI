using EmpregaNet.Application.Users.Commands;
using FluentAssertions;
using FluentValidation;

namespace EmpregaNet.Tests.Unit.Application.Users;

/// <summary>
/// Objetivo: garantir regras de entrada do cadastro (senha forte, e-mail, telefone BR opcional)
/// alinhadas à política de domínio, sem depender de HTTP ou Identity.
/// </summary>
public sealed class RegisterUserCommandValidatorTests
{
    private readonly IValidator<RegisterUserCommand> _sut = new RegisterUserCommandValidator();

    [Fact]
    public void Validate_SenhaSemCaracterEspecial_DeveFalhar()
    {
        // Arrange
        var cmd = CreateValidCommand() with { Password = "Abcdef12", PasswordConfirmation = "Abcdef12" };

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Fact]
    public void Validate_SenhaValidaEConfirmacaoIgual_DevePassar()
    {
        // Arrange
        var cmd = CreateValidCommand();

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmailInvalido_DeveFalhar()
    {
        // Arrange
        var cmd = CreateValidCommand() with { Email = "nao-e-email" };

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Fact]
    public void Validate_CelularBrasilFormatoInvalido_DeveFalhar()
    {
        // Arrange — DDD + número sem o 9 inicial exigido pela regra BR do projeto
        var cmd = CreateValidCommand() with { PhoneNumber = "551198765432" };

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.PhoneNumber));
    }

    [Fact]
    public void Validate_CelularBrasilValido_DevePassar()
    {
        // Arrange — 55 + DDD + 9 + 8 dígitos (regra em FluentValidationExtensions)
        var cmd = CreateValidCommand() with { PhoneNumber = "5511987654321" };

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TelefoneVazio_DeveIgnorarRegraDeCelularEPassar()
    {
        // Arrange
        var cmd = CreateValidCommand() with { PhoneNumber = null };

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    private static RegisterUserCommand CreateValidCommand() =>
        new(
            Username: "candidato1",
            Email: "candidato@test.local",
            Password: "Abcd@123",
            PasswordConfirmation: "Abcd@123",
            PhoneNumber: null);
}

using EmpregaNet.Application.Users.Commands;
using FluentAssertions;
using FluentValidation;

namespace EmpregaNet.Tests.Unit.Application.Users;

/// <summary>
/// Objetivo: validar contrato de entrada do reset de senha (userId, token, política de senha nova),
/// espelhando o que o pipeline rejeita antes de tocar no UserManager.
/// </summary>
public sealed class ResetPasswordCommandValidatorTests
{
    private readonly IValidator<ResetPasswordCommand> _sut = new ResetPasswordCommandValidator();

    [Fact]
    public void Validate_UserIdZero_DeveFalhar()
    {
        // Arrange
        var cmd = new ResetPasswordCommand(0, "token", "Abcd@123", "Abcd@123");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.UserId));
    }

    [Fact]
    public void Validate_TokenVazio_DeveFalhar()
    {
        // Arrange
        var cmd = new ResetPasswordCommand(1L, "", "Abcd@123", "Abcd@123");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.Token));
    }

    [Fact]
    public void Validate_NovaSenhaSemDigito_DeveFalhar()
    {
        // Arrange
        var cmd = new ResetPasswordCommand(1L, "tok", "Abcdefg@", "Abcdefg@");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_ComandoValido_DevePassar()
    {
        // Arrange
        var cmd = new ResetPasswordCommand(42L, "any-token", "Zyxw9@ab", "Zyxw9@ab");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

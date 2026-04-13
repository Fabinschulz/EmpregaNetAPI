using EmpregaNet.Application.Users.Commands;
using FluentAssertions;
using FluentValidation;

namespace EmpregaNet.Tests.Unit;

/// <summary>
/// Objetivo: garantir que confirmação de e-mail não aceita identificadores ou tokens vazios
/// (evita chamadas inúteis ao Identity com dados inválidos).
/// </summary>
public sealed class ConfirmEmailCommandValidatorTests
{
    private readonly IValidator<ConfirmEmailCommand> _sut = new ConfirmEmailCommandValidator();

    [Fact]
    public void Validate_UserIdZero_DeveFalhar()
    {
        // Arrange
        var cmd = new ConfirmEmailCommand(0, "token");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ConfirmEmailCommand.UserId));
    }

    [Fact]
    public void Validate_TokenVazio_DeveFalhar()
    {
        // Arrange
        var cmd = new ConfirmEmailCommand(10L, "");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ConfirmEmailCommand.Token));
    }

    [Fact]
    public void Validate_EntradaValida_DevePassar()
    {
        // Arrange
        var cmd = new ConfirmEmailCommand(1L, "CfDJ8...");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

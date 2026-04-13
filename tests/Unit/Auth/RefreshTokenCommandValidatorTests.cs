using EmpregaNet.Application.Users.Commands;
using FluentAssertions;
using FluentValidation;

namespace EmpregaNet.Tests.Unit;

/// <summary>
/// Objetivo: garantir que o comando de refresh não segue para o handler com token vazio
/// (evita chamadas desnecessárias ao serviço de rotação).
/// </summary>
public sealed class RefreshTokenCommandValidatorTests
{
    private readonly IValidator<RefreshTokenCommand> _sut = new RefreshTokenCommandValidator();

    [Fact]
    public void Validate_RefreshTokenVazio_DeveFalhar()
    {
        // Arrange
        var cmd = new RefreshTokenCommand("");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RefreshTokenCommand.RefreshToken));
    }

    [Fact]
    public void Validate_RefreshTokenPreenchido_DevePassar()
    {
        // Arrange
        var cmd = new RefreshTokenCommand("token-opaco");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

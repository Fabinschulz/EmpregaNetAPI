using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.Auth;

/// <summary>
/// Objetivo: validar a regra de negócio do refresh — rotação delegada ao serviço, rejeição se token
/// inválido ou utilizador apagado logicamente, e anexação do novo refresh ao view model.
/// </summary>
public sealed class RefreshTokenHandlerTests
{
    private readonly Mock<IRefreshTokenService> _refresh = new();
    private readonly Mock<IJwtBuilder> _jwt = new();

    [Fact]
    public async Task Handle_RotateAsyncRetornaNull_DeveLancarValidationAppException()
    {
        // Given
        _refresh
            .Setup(x => x.RotateAsync("expirado", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ValueTuple<User, string>?)null);
        var sut = new RefreshTokenHandler(_refresh.Object, _jwt.Object);
        var cmd = new RefreshTokenCommand("expirado");

        // When
        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_FORM
                        && e.Errors.ContainsKey(nameof(RefreshTokenCommand.RefreshToken)));
        _jwt.Verify(x => x.BuildUserTokenAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UtilizadorMarcadoComoDeletado_DeveLancarValidationAppException()
    {
        // Given
        var user = new User { Id = 7, UserName = "x", Email = "x@test", IsDeleted = true };
        _refresh
            .Setup(x => x.RotateAsync("ok", It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, "novo-refresh"));
        var sut = new RefreshTokenHandler(_refresh.Object, _jwt.Object);

        // When
        var act = async () => await sut.Handle(new RefreshTokenCommand("ok"), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_FORM);
        _jwt.Verify(x => x.BuildUserTokenAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RotacaoValida_DeveEmitirJwtEAtribuirNovoRefreshToken()
    {
        // Given
        var user = new User { Id = 3, UserName = "ana", Email = "ana@test", IsDeleted = false };
        _refresh
            .Setup(x => x.RotateAsync("velho", It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, "novo-opaco"));
        var vm = new UserLoggedViewModel
        {
            AccessToken = "Bearer t",
            ExpiresIn = 3600,
            UserToken = new UserToken
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                Claims = []
            }
        };
        _jwt.Setup(x => x.BuildUserTokenAsync(user)).ReturnsAsync(vm);
        var sut = new RefreshTokenHandler(_refresh.Object, _jwt.Object);

        // When
        var result = await sut.Handle(new RefreshTokenCommand("velho"), CancellationToken.None);

        // Then
        result.RefreshToken.Should().Be("novo-opaco");
        result.AccessToken.Should().Be("Bearer t");
        _refresh.Verify(x => x.RotateAsync("velho", It.IsAny<CancellationToken>()), Times.Once);
        _jwt.Verify(x => x.BuildUserTokenAsync(user), Times.Once);
    }
}

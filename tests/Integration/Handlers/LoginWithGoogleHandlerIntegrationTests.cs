using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Objetivo: validar rejeições precoces do login Google (token inválido / e-mail não verificado na Google) sem depender do SDK Google.
/// </summary>
[Collection("Integration")]
public sealed class LoginWithGoogleHandlerIntegrationTests : IDisposable
{
    private readonly InMemoryIdentityFixture _fx;

    public LoginWithGoogleHandlerIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
        _fx.ResetMocks();
    }

    public void Dispose() => _fx.ResetMocks();

    [Fact]
    public async Task Handle_IdTokenInvalido_DeveLancarValidationAppException()
    {
        _fx.Google.Setup(x => x.ValidateAsync("bad", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GoogleIdTokenPayload?)null);

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginWithGoogleHandler>();

        var act = async () => await sut.Handle(new LoginWithGoogleCommand("bad"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_PARAMS);
    }

    [Fact]
    public async Task Handle_EmailGoogleNaoVerificado_DeveLancarValidationAppException()
    {
        _fx.Google.Setup(x => x.ValidateAsync("tok", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleIdTokenPayload("sub-1", "a@test.local", EmailVerified: false));

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginWithGoogleHandler>();

        var act = async () => await sut.Handle(new LoginWithGoogleCommand("tok"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_PARAMS);
    }

    [Fact]
    public async Task Handle_NovoUtilizadorGoogle_DeveCriarContaEemitirJwt()
    {
        var email = $"gg_{Guid.NewGuid():N}@test.local";
        _fx.Google.Setup(x => x.ValidateAsync("good-tok", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleIdTokenPayload($"sub-{Guid.NewGuid():N}", email, EmailVerified: true));

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginWithGoogleHandler>();

        var vm = await sut.Handle(new LoginWithGoogleCommand("good-tok"), CancellationToken.None);

        vm.UserToken.Email.Should().Be(email);
        vm.AccessToken.Should().StartWith("Bearer ");
        vm.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }
}

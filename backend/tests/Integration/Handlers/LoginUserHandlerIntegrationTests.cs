using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Objetivo: validar login por senha com Identity (utilizador inexistente, senha errada, e-mail não confirmado, sucesso após confirmação).
/// </summary>
[Collection("Integration")]
public sealed class LoginUserHandlerIntegrationTests : IDisposable
{
    private readonly InMemoryIdentityFixture _fx;

    public LoginUserHandlerIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
        _fx.ResetMocks();
    }

    public void Dispose() => _fx.ResetMocks();

    [Fact]
    public async Task Handle_EmailInexistente_DeveLancarValidationAppException()
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginUserHandler>();

        var act = async () => await sut.Handle(
            new LoginUserCommand(TestDataFactory.UniqueEmail("ghost"), AuthIntegrationTestHelper.DefaultPassword),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_PASSWORD);
    }

    [Fact]
    public async Task Handle_SenhaIncorreta_DeveLancarValidationAppException()
    {
        var email = TestDataFactory.UniqueEmail("login_wrong");
        await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "login_wrong");

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginUserHandler>();

        var act = async () => await sut.Handle(new LoginUserCommand(email, "Errada1@x"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_PASSWORD);
    }

    [Fact]
    public async Task Handle_EmailNaoConfirmado_DeveLancarValidationAppException()
    {
        var email = TestDataFactory.UniqueEmail("unconf");
        using (var scope = _fx.Services.CreateScope())
        {
            var register = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();
            await register.Handle(
                new RegisterUserCommand(
                    TestDataFactory.UniqueUsername("unconf"),
                    email,
                    AuthIntegrationTestHelper.DefaultPassword,
                    AuthIntegrationTestHelper.DefaultPassword,
                    TestDataFactory.UniqueBrazilianCell()),
                CancellationToken.None);
        }

        await using var scope2 = _fx.Services.CreateAsyncScope();
        var sut = scope2.ServiceProvider.GetRequiredService<LoginUserHandler>();

        var act = async () => await sut.Handle(new LoginUserCommand(email, AuthIntegrationTestHelper.DefaultPassword), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
    }

    [Fact]
    public async Task Handle_AposConfirmacaoCredenciaisValidas_DeveRetornarJwtERefreshToken()
    {
        var email = TestDataFactory.UniqueEmail("login_ok");
        await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "login_ok");

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginUserHandler>();

        var vm = await sut.Handle(new LoginUserCommand(email, AuthIntegrationTestHelper.DefaultPassword), CancellationToken.None);

        vm.AccessToken.Should().StartWith("Bearer ");
        vm.RefreshToken.Should().NotBeNullOrWhiteSpace();
        vm.UserToken.Email.Should().Be(email);
    }
}

using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Objetivo: validar troca de senha autenticada (contexto simulado via <see cref="IHttpCurrentUser"/>).
/// </summary>
[Collection("Integration")]
public sealed class ChangeMyPasswordHandlerIntegrationTests : IDisposable
{
    private readonly InMemoryIdentityFixture _fx;

    public ChangeMyPasswordHandlerIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
        _fx.ResetMocks();
    }

    public void Dispose()
    {
        _fx.ResetMocks();
        _fx.HttpUser.Reset();
    }

    [Fact]
    public async Task Handle_SenhaAtualIncorreta_DeveLancarValidationAppException()
    {
        var email = TestDataFactory.UniqueEmail("chg");
        var id = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "chg");

        _fx.HttpUser.Setup(x => x.UserId).Returns(id);

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<ChangeMyPasswordHandler>();

        var act = async () => await sut.Handle(
            new ChangeMyPasswordCommand("Teste@12", "Nova1@abc", "Nova1@abc"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_PASSWORD);
    }

    [Fact]
    public async Task Handle_SenhaAtualCorreta_DeveConcluirEInvalidarRefreshTokens()
    {
        var email = TestDataFactory.UniqueEmail("chg_ok");
        var id = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "chg_ok");

        _fx.HttpUser.Setup(x => x.UserId).Returns(id);

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<ChangeMyPasswordHandler>();

        await sut.Handle(
            new ChangeMyPasswordCommand(AuthIntegrationTestHelper.DefaultPassword, "Nova2@ab", "Nova2@ab"),
            CancellationToken.None);

        await using var scope2 = _fx.Services.CreateAsyncScope();
        var login = scope2.ServiceProvider.GetRequiredService<LoginUserHandler>();
        var vm = await login.Handle(new LoginUserCommand(email, "Nova2@ab"), CancellationToken.None);
        vm.AccessToken.Should().StartWith("Bearer ");
    }
}

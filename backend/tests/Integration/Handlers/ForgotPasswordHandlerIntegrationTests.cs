using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Objetivo: garantir mensagem uniforme quando não há conta e envio de e-mail quando há utilizador ativo.
/// </summary>
[Collection("Integration")]
public sealed class ForgotPasswordHandlerIntegrationTests : IDisposable
{
    private readonly InMemoryIdentityFixture _fx;

    public ForgotPasswordHandlerIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
        _fx.ResetMocks();
    }

    public void Dispose() => _fx.ResetMocks();

    [Fact]
    public async Task Handle_EmailSemConta_DeveRetornarMensagemPublicaSemEnviarReset()
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<ForgotPasswordHandler>();

        var result = await sut.Handle(new ForgotPasswordCommand(TestDataFactory.UniqueEmail("naoexiste")), CancellationToken.None);

        result.Message.Should().Contain("instruções");
        _fx.AccountEmail.Verify(
            x => x.SendPasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ContaConfirmadaExiste_DeveChamarEnvioDeReset()
    {
        var email = TestDataFactory.UniqueEmail("forgot");
        await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "forgot");
        _fx.AccountEmail.Invocations.Clear();

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<ForgotPasswordHandler>();

        var result = await sut.Handle(new ForgotPasswordCommand(email), CancellationToken.None);

        result.Message.Should().Contain("instruções");
        _fx.AccountEmail.Verify(
            x => x.SendPasswordResetLinkAsync(email, It.Is<string>(l => l.Contains("userId=") && l.Contains("token=")), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Objetivo: garantir que o reenvio só tenta SMTP quando há conta pendente de confirmação.
/// </summary>
[Collection("Integration")]
public sealed class ResendEmailConfirmationHandlerIntegrationTests : IDisposable
{
    private readonly InMemoryIdentityFixture _fx;

    public ResendEmailConfirmationHandlerIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
        _fx.ResetMocks();
    }

    public void Dispose() => _fx.ResetMocks();

    [Fact]
    public async Task Handle_EmailSemConta_DeveRetornarMensagemPublicaSemEnviar()
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<ResendEmailConfirmationHandler>();

        var result = await sut.Handle(new ResendEmailConfirmationCommand(TestDataFactory.UniqueEmail("inexistente")), CancellationToken.None);

        result.Message.Should().Contain("pendente");
        _fx.AccountEmail.Verify(
            x => x.SendEmailConfirmationLinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ContaJaConfirmada_DeveRetornarMensagemPublicaSemEnviar()
    {
        var email = TestDataFactory.UniqueEmail("resend_done");
        await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "resend_done");
        _fx.AccountEmail.Invocations.Clear();

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<ResendEmailConfirmationHandler>();

        var result = await sut.Handle(new ResendEmailConfirmationCommand(email), CancellationToken.None);

        result.Message.Should().Contain("pendente");
        _fx.AccountEmail.Verify(
            x => x.SendEmailConfirmationLinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ContaPendenteConfirmacao_DeveEnviarNovoLink()
    {
        var email = TestDataFactory.UniqueEmail("resend_pend");
        using (var scope = _fx.Services.CreateScope())
        {
            var reg = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();
            await reg.Handle(
                new RegisterUserCommand(
                    TestDataFactory.UniqueUsername("resend_pend"),
                    email,
                    AuthIntegrationTestHelper.DefaultPassword,
                    AuthIntegrationTestHelper.DefaultPassword,
                    TestDataFactory.UniqueBrazilianCell()),
                CancellationToken.None);
        }

        _fx.AccountEmail.Invocations.Clear();

        await using var scope2 = _fx.Services.CreateAsyncScope();
        var sut = scope2.ServiceProvider.GetRequiredService<ResendEmailConfirmationHandler>();

        var result = await sut.Handle(new ResendEmailConfirmationCommand(email), CancellationToken.None);

        result.Message.Should().Contain("pendente");
        _fx.AccountEmail.Verify(
            x => x.SendEmailConfirmationLinkAsync(email, It.Is<string>(l => l.Contains("userId=")), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

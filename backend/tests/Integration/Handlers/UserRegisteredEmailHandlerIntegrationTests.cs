using EmpregaNet.Application.Auth.Commands;
using EmpregaNet.Application.Auth.Events;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// O e-mail de confirmação de conta sai <b>aqui</b>, depois do commit, e não no
/// <c>RegisterUserHandler</c>.
/// </summary>
/// <remarks>
/// Precisa do Identity real: o link carrega um token gerado por
/// <c>GenerateEmailConfirmationTokenAsync</c> contra o utilizador lido da base, e o que importa fixar é
/// que esse link continua a ser aceitável por <c>ConfirmEmailHandler</c> — a mudança de sítio do envio
/// não pode ter partido o fluxo de confirmação.
/// </remarks>
[Collection("Integration")]
public sealed class UserRegisteredEmailHandlerIntegrationTests : IDisposable
{
    private readonly InMemoryIdentityFixture _fx;

    public UserRegisteredEmailHandlerIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
        _fx.ResetMocks();
    }

    public void Dispose() => _fx.ResetMocks();

    private async Task<long> RegisterPendingUserAsync(string email)
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var register = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();

        return await register.Handle(
            new RegisterUserCommand(
                TestDataFactory.UniqueUsername("evt"),
                email,
                TestDataFactory.UniqueCpf(),
                AuthIntegrationTestHelper.DefaultPassword,
                AuthIntegrationTestHelper.DefaultPassword,
                TestDataFactory.UniqueBrazilianCell()),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ContaCriada_DeveEnviarLinkComUserIdEToken()
    {
        var email = TestDataFactory.UniqueEmail("evt_ok");
        var id = await RegisterPendingUserAsync(email);
        _fx.AccountEmail.Invocations.Clear();

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<UserRegisteredEmailHandler>();

        await sut.Handle(new UserRegistered(id), CancellationToken.None);

        _fx.AccountEmail.Verify(
            x => x.SendEmailConfirmationLinkAsync(
                email,
                It.Is<string>(link => link.Contains($"userId={id}") && link.Contains("token=")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// O token do link é aceito pela confirmação: o envio pós-commit continua a produzir um link
    /// utilizável, não só uma string com a forma certa.
    /// </summary>
    [Fact]
    public async Task Handle_ContaCriada_DeveProduzirLinkQueConfirmaOEmail()
    {
        var email = TestDataFactory.UniqueEmail("evt_confirma");
        var id = await RegisterPendingUserAsync(email);
        _fx.AccountEmail.Invocations.Clear();

        string? sentLink = null;
        _fx.AccountEmail
            .Setup(x => x.SendEmailConfirmationLinkAsync(email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, link, _) => sentLink = link)
            .Returns(Task.CompletedTask);

        await using var scope = _fx.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<UserRegisteredEmailHandler>()
            .Handle(new UserRegistered(id), CancellationToken.None);

        sentLink.Should().NotBeNull();
        var token = Uri.UnescapeDataString(sentLink!.Split("&token=")[1]);

        var confirm = scope.ServiceProvider.GetRequiredService<ConfirmEmailHandler>();
        await confirm.Handle(new ConfirmEmailCommand(id, token), CancellationToken.None);

        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var confirmed = await users.FindByIdAsync(id.ToString());
        confirmed!.EmailConfirmed.Should().BeTrue();
    }

    /// <summary>
    /// Conta inexistente degrada para log e não relança: o registo já respondeu com êxito e uma
    /// excepção aqui não tem quem a trate.
    /// </summary>
    [Fact]
    public async Task Handle_ContaInexistente_NaoDeveEnviarNemRelancar()
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<UserRegisteredEmailHandler>();

        var act = async () => await sut.Handle(new UserRegistered(long.MaxValue), CancellationToken.None);

        await act.Should().NotThrowAsync();
        _fx.AccountEmail.Verify(
            x => x.SendEmailConfirmationLinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>Falha do transporte não sobe: o handler garante-o por si, não por quem o invoca.</summary>
    [Fact]
    public async Task Handle_FalhaNoEnvio_NaoDeveRelancar()
    {
        var email = TestDataFactory.UniqueEmail("evt_falha");
        var id = await RegisterPendingUserAsync(email);
        _fx.AccountEmail.Invocations.Clear();

        _fx.AccountEmail
            .Setup(x => x.SendEmailConfirmationLinkAsync(email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP indisponível"));

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<UserRegisteredEmailHandler>();

        var act = async () => await sut.Handle(new UserRegistered(id), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}

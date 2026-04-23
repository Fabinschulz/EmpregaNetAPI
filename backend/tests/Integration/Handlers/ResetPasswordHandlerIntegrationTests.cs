using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Objetivo: validar reset de senha (confirmação divergente e fluxo feliz com token real do Identity).
/// </summary>
[Collection("Integration")]
public sealed class ResetPasswordHandlerIntegrationTests : IDisposable
{
    private readonly InMemoryIdentityFixture _fx;

    public ResetPasswordHandlerIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
        _fx.ResetMocks();
    }

    public void Dispose() => _fx.ResetMocks();

    [Fact]
    public async Task Handle_ConfirmacaoSenhaDiferente_DeveLancarValidationAppException()
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<ResetPasswordHandler>();

        var act = async () => await sut.Handle(
            new ResetPasswordCommand(1, "tok", "Nova1@ab", "Nova1@ac"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_PARAMS);
    }

    [Fact]
    public async Task Handle_TokenValido_DevePermitirLoginComNovaSenha()
    {
        var email = TestDataFactory.UniqueEmail("reset");
        long id;
        string resetToken;
        using (var scope = _fx.Services.CreateScope())
        {
            var reg = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();
            id = await reg.Handle(
                new RegisterUserCommand(
                    TestDataFactory.UniqueUsername("reset"),
                    email,
                    AuthIntegrationTestHelper.DefaultPassword,
                    AuthIntegrationTestHelper.DefaultPassword,
                    TestDataFactory.UniqueBrazilianCell()),
                CancellationToken.None);

            var um = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var u = await um.FindByIdAsync(id.ToString());
            var confirmTok = await um.GenerateEmailConfirmationTokenAsync(u!);
            await scope.ServiceProvider.GetRequiredService<ConfirmEmailHandler>()
                .Handle(new ConfirmEmailCommand(id, confirmTok), CancellationToken.None);

            u = await um.FindByIdAsync(id.ToString());
            resetToken = await um.GeneratePasswordResetTokenAsync(u!);
        }

        const string newPass = "Zyxw9@mn";
        await using var scope3 = _fx.Services.CreateAsyncScope();
        var sut = scope3.ServiceProvider.GetRequiredService<ResetPasswordHandler>();
        await sut.Handle(new ResetPasswordCommand(id, resetToken, newPass, newPass), CancellationToken.None);

        await using var scope4 = _fx.Services.CreateAsyncScope();
        var login = scope4.ServiceProvider.GetRequiredService<LoginUserHandler>();
        var vm = await login.Handle(new LoginUserCommand(email, newPass), CancellationToken.None);
        vm.AccessToken.Should().StartWith("Bearer ");
    }
}

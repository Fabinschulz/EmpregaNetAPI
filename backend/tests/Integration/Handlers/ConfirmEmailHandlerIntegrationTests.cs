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
/// Objetivo: validar confirmação de e-mail com tokens reais do Identity (falha e sucesso).
/// </summary>
[Collection("Integration")]
public sealed class ConfirmEmailHandlerIntegrationTests : IDisposable
{
    private readonly InMemoryIdentityFixture _fx;

    public ConfirmEmailHandlerIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
        _fx.ResetMocks();
    }

    public void Dispose() => _fx.ResetMocks();

    [Fact]
    public async Task Handle_UtilizadorInexistente_DeveLancarValidationAppException()
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<ConfirmEmailHandler>();

        var act = async () => await sut.Handle(new ConfirmEmailCommand(999_999, "token"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_FORM);
    }

    [Fact]
    public async Task Handle_TokenInvalido_DeveLancarValidationAppException()
    {
        var email = TestDataFactory.UniqueEmail("ce_bad");
        long id;
        using (var scope = _fx.Services.CreateScope())
        {
            var reg = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();
            id = await reg.Handle(
                new RegisterUserCommand(
                    TestDataFactory.UniqueUsername("ce_bad"),
                    email,
                    AuthIntegrationTestHelper.DefaultPassword,
                    AuthIntegrationTestHelper.DefaultPassword,
                    TestDataFactory.UniqueBrazilianCell()),
                CancellationToken.None);
        }

        await using var scope2 = _fx.Services.CreateAsyncScope();
        var sut = scope2.ServiceProvider.GetRequiredService<ConfirmEmailHandler>();

        var act = async () => await sut.Handle(new ConfirmEmailCommand(id, "token-invalido"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_FORM);
    }

    [Fact]
    public async Task Handle_TokenValido_DeveMarcarEmailComoConfirmado()
    {
        var email = TestDataFactory.UniqueEmail("ce_ok");
        long id;
        string token;
        using (var scope = _fx.Services.CreateScope())
        {
            var reg = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();
            id = await reg.Handle(
                new RegisterUserCommand(
                    TestDataFactory.UniqueUsername("ce_ok"),
                    email,
                    AuthIntegrationTestHelper.DefaultPassword,
                    AuthIntegrationTestHelper.DefaultPassword,
                    TestDataFactory.UniqueBrazilianCell()),
                CancellationToken.None);
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var u = await users.FindByIdAsync(id.ToString());
            token = await users.GenerateEmailConfirmationTokenAsync(u!);
            var sut = scope.ServiceProvider.GetRequiredService<ConfirmEmailHandler>();
            await sut.Handle(new ConfirmEmailCommand(id, token), CancellationToken.None);
        }

        using var verifyScope = _fx.Services.CreateScope();
        var um = verifyScope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var refreshed = await um.FindByIdAsync(id.ToString());
        refreshed.Should().NotBeNull();
        refreshed!.EmailConfirmed.Should().BeTrue();
    }
}

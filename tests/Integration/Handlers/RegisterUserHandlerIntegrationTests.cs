using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Objetivo: validar regras de negócio do registo (confirmação de senha, unicidade, criação) com Identity real em memória.
/// </summary>
[Collection("Integration")]
public sealed class RegisterUserHandlerIntegrationTests : IDisposable
{
    private readonly InMemoryIdentityFixture _fx;

    public RegisterUserHandlerIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
        _fx.ResetMocks();
    }

    public void Dispose() => _fx.ResetMocks();

    [Fact]
    public async Task Handle_SenhasDiferentes_DeveLancarValidationAppException()
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();
        var cmd = new RegisterUserCommand("u1", "a@test.local", AuthIntegrationTestHelper.DefaultPassword, "Outra1@xy", TestDataFactory.UniqueBrazilianCell());

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_PARAMS);
    }

    [Fact]
    public async Task Handle_EmailDuplicado_DeveLancarValidationAppException()
    {
        var email = TestDataFactory.UniqueEmail("dup");
        await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "dup");

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();
        var cmd = new RegisterUserCommand(
            TestDataFactory.UniqueUsername("dup2"),
            email,
            AuthIntegrationTestHelper.DefaultPassword,
            AuthIntegrationTestHelper.DefaultPassword,
            TestDataFactory.UniqueBrazilianCell());

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
    }

    [Fact]
    public async Task Handle_RegistoValido_DeveRetornarIdEChamarEnvioDeConfirmacao()
    {
        var email = TestDataFactory.UniqueEmail("ok");
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();

        var id = await sut.Handle(
            new RegisterUserCommand(
                TestDataFactory.UniqueUsername("novo"),
                email,
                AuthIntegrationTestHelper.DefaultPassword,
                AuthIntegrationTestHelper.DefaultPassword,
                TestDataFactory.UniqueBrazilianCell()),
            CancellationToken.None);

        id.Should().BeGreaterThan(0);
        _fx.AccountEmail.Verify(
            x => x.SendEmailConfirmationLinkAsync(email, It.Is<string>(link => link.Contains($"userId={id}")), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

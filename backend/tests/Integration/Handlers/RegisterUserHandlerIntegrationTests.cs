using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Auth.Commands;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
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
        var cmd = new RegisterUserCommand("u1", "a@test.local", TestDataFactory.UniqueCpf(), AuthIntegrationTestHelper.DefaultPassword, "Outra1@xy", TestDataFactory.UniqueBrazilianCell());

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
            TestDataFactory.UniqueCpf(),
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
                TestDataFactory.UniqueCpf(),
                AuthIntegrationTestHelper.DefaultPassword,
                AuthIntegrationTestHelper.DefaultPassword,
                TestDataFactory.UniqueBrazilianCell()),
            CancellationToken.None);

        id.Should().BeGreaterThan(0);
        _fx.AccountEmail.Verify(
            x => x.SendEmailConfirmationLinkAsync(email, It.Is<string>(link => link.Contains($"userId={id}")), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RegistoValido_DeveGravarOCpfSemMascara()
    {
        var cpf = TestDataFactory.UniqueCpf();
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();

        var id = await sut.Handle(
            new RegisterUserCommand(
                TestDataFactory.UniqueUsername("cpf_ok"),
                TestDataFactory.UniqueEmail("cpf_ok"),
                TestDataFactory.MaskCpf(cpf),
                AuthIntegrationTestHelper.DefaultPassword,
                AuthIntegrationTestHelper.DefaultPassword,
                TestDataFactory.UniqueBrazilianCell()),
            CancellationToken.None);

        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var created = await users.FindByIdAsync(id.ToString());

        created!.Cpf.Should().Be(cpf, "a máscara é da apresentação, o armazenamento é canónico");
    }

    [Theory]
    [InlineData(false, "o mesmo CPF sem máscara")]
    [InlineData(true, "o mesmo CPF com máscara diferente")]
    public async Task Handle_CpfDuplicado_DeveLancarValidationAppException(bool masked, string cenario)
    {
        var cpf = TestDataFactory.UniqueCpf();
        await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(
            _fx.Services, TestDataFactory.UniqueEmail("cpf_dup"), "cpf_dup", cpf);

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();
        var cmd = new RegisterUserCommand(
            TestDataFactory.UniqueUsername("cpf_dup2"),
            TestDataFactory.UniqueEmail("cpf_dup2"),
            masked ? TestDataFactory.MaskCpf(cpf) : cpf,
            AuthIntegrationTestHelper.DefaultPassword,
            AuthIntegrationTestHelper.DefaultPassword,
            TestDataFactory.UniqueBrazilianCell());

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<ValidationAppException>(cenario);

        thrown.Which.Code.Should().Be(
            DomainErrorEnum.RESOURCE_CREATION_FAILED,
            "RESOURCE_ALREADY_EXISTS confirmaria que aquele CPF está cadastrado");

        thrown.Which.Errors.Keys.Should().OnlyContain(
            key => key == string.Empty,
            "atribuir o erro ao campo Cpf aponta a causa e permite enumeração");

        thrown.Which.Errors.SelectMany(e => e.Value).Should().NotContain(
            message => message.Contains("CPF", StringComparison.OrdinalIgnoreCase),
            "a mensagem devolvida ao cliente não pode citar o CPF");
    }
}

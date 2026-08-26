using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Auth.Commands;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Objetivo: validar login por senha com Identity (utilizador inexistente, senha errada, e-mail não
/// confirmado, sucesso após confirmação) e a resolução do identificador por <b>CPF ou e-mail</b>,
/// incluindo a garantia de que nome de usuário não autentica.
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
                    TestDataFactory.UniqueCpf(),
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

    [Fact]
    public async Task Handle_CpfSemMascara_DeveAutenticarEEmitirToken()
    {
        var email = TestDataFactory.UniqueEmail("login_cpf");
        var cpf = TestDataFactory.UniqueCpf();
        await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "login_cpf", cpf);

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginUserHandler>();

        var vm = await sut.Handle(new LoginUserCommand(cpf, AuthIntegrationTestHelper.DefaultPassword), CancellationToken.None);

        vm.AccessToken.Should().StartWith("Bearer ");
        vm.RefreshToken.Should().NotBeNullOrWhiteSpace();
        vm.UserToken.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_CpfComMascara_DeveAutenticarComoSeFosseNormalizado()
    {
        // O CPF é guardado só com dígitos: se a busca não normalizasse a entrada, a mesma pessoa
        // conseguiria entrar digitando 12345678900 e falharia digitando 123.456.789-00.
        var email = TestDataFactory.UniqueEmail("login_cpf_mask");
        var cpf = TestDataFactory.UniqueCpf();
        await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "login_cpf_mask", cpf);

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginUserHandler>();

        var vm = await sut.Handle(
            new LoginUserCommand(TestDataFactory.MaskCpf(cpf), AuthIntegrationTestHelper.DefaultPassword),
            CancellationToken.None);

        vm.UserToken.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_CpfCorretoESenhaErrada_DeveLancarValidationAppException()
    {
        var email = TestDataFactory.UniqueEmail("login_cpf_pwd");
        var cpf = TestDataFactory.UniqueCpf();
        await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "login_cpf_pwd", cpf);

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginUserHandler>();

        var act = async () => await sut.Handle(new LoginUserCommand(cpf, "Errada1@x"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_PASSWORD);
    }

    [Fact]
    public async Task Handle_CpfInexistente_DeveLancarOMesmoErroDeSenhaInvalida()
    {
        // Mesmo código de erro do e-mail inexistente e da senha errada: a resposta não diz se o CPF
        // está cadastrado, senão o endpoint viraria um verificador de CPFs.
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginUserHandler>();

        var act = async () => await sut.Handle(
            new LoginUserCommand(TestDataFactory.UniqueCpf(), AuthIntegrationTestHelper.DefaultPassword),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_PASSWORD);
    }

    [Fact]
    public async Task Handle_NomeDeUsuarioComoIdentificador_NaoDeveAutenticar()
    {
        // A conta existe e a senha está correta; só o identificador é o nome de usuário. Antes desta
        // mudança este cenário autenticava — é exactamente o fallback que deixou de existir.
        var email = TestDataFactory.UniqueEmail("login_user");
        var id = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "login_user");

        await using var scope = _fx.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var entity = await users.FindByIdAsync(id.ToString());
        var username = entity!.UserName!;

        var sut = scope.ServiceProvider.GetRequiredService<LoginUserHandler>();

        var act = async () => await sut.Handle(
            new LoginUserCommand(username, AuthIntegrationTestHelper.DefaultPassword),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_PASSWORD);
    }

    [Fact]
    public async Task Handle_ContaSemCpf_DeveContinuarAAutenticarPorEmail()
    {
        // Contas anteriores à coluna ficam com CPF nulo: o login por e-mail não pode regredir.
        var email = TestDataFactory.UniqueEmail("legacy");
        var id = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, "legacy");

        using (var arrange = _fx.Services.CreateScope())
        {
            var users = arrange.ServiceProvider.GetRequiredService<UserManager<User>>();
            var entity = await users.FindByIdAsync(id.ToString());
            entity!.Cpf = null;
            await users.UpdateAsync(entity);
        }

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginUserHandler>();

        var vm = await sut.Handle(new LoginUserCommand(email, AuthIntegrationTestHelper.DefaultPassword), CancellationToken.None);

        vm.AccessToken.Should().StartWith("Bearer ");
        vm.UserToken.Email.Should().Be(email);
    }
}

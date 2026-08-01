using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth.Commands;
using EmpregaNet.Application.Users.Identity;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Objetivo: garantir que o login Google deixa sempre o utilizador com a role de candidato.
/// </summary>
/// <remarks>
/// Estes cenários cobrem um defeito concreto: a atribuição da role só acontecia no caminho de
/// criação, e o resultado da operação era descartado. Um utilizador que ficasse sem role nunca a
/// recebia nenhum login posterior repetia a tentativa. Os testes existentes de login Google
/// verificavam e-mail e tokens, mas nunca a role, e por isso não apanhavam o problema.
/// </remarks>
[Collection("Integration")]
public sealed class LoginWithGoogleRoleIntegrationTests : IDisposable
{
    private readonly InMemoryIdentityFixture _fx;

    public LoginWithGoogleRoleIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
        _fx.ResetMocks();
    }

    public void Dispose() => _fx.ResetMocks();

    private void SetupGoogle(string token, string subject, string email, bool emailVerified = true)
        => _fx.Google
            .Setup(x => x.ValidateAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleIdTokenPayload(subject, email, emailVerified));

    private async Task<UserLoggedViewModelProbe> LoginAsync(string token)
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<LoginWithGoogleHandler>();
        var vm = await sut.Handle(new LoginWithGoogleCommand(token), CancellationToken.None);
        return new UserLoggedViewModelProbe(vm.UserToken.Email, vm.AccessToken, vm.RefreshToken);
    }

    private async Task<IList<string>> GetRolesAsync(string email)
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await users.FindByEmailAsync(email);
        user.Should().NotBeNull("o utilizador devia existir após o login Google");
        return await users.GetRolesAsync(user!);
    }

    [Fact]
    public async Task Handle_NovoUtilizadorViaGoogle_DeveTerminarComRoleCandidate()
    {
        var email = TestDataFactory.UniqueEmail("gg_role_novo");
        SetupGoogle("tok-novo", $"sub-{Guid.NewGuid():N}", email);

        await LoginAsync("tok-novo");

        var roles = await GetRolesAsync(email);
        roles.Should().Contain(CandidateRoleAssignment.RoleName);
    }

    [Fact]
    public async Task Handle_ContaExistenteSemRole_DeveGarantirRoleNoLoginGoogle()
    {
        // Cria a conta diretamente pelo UserManager, deliberadamente SEM role: simula o utilizador
        // que ficou sem role por causa do defeito antigo.
        var email = TestDataFactory.UniqueEmail("gg_role_repara");
        await using (var scope = _fx.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var created = await users.CreateAsync(new User
            {
                UserName = TestDataFactory.UniqueUsername("gg_role_repara"),
                Email = email,
                EmailConfirmed = true,
                UserType = UserTypeEnum.Candidate
            });
            created.Succeeded.Should().BeTrue();
        }

        (await GetRolesAsync(email)).Should().BeEmpty("o arranjo do teste depende de a conta começar sem role");

        SetupGoogle("tok-repara", $"sub-{Guid.NewGuid():N}", email);
        await LoginAsync("tok-repara");

        (await GetRolesAsync(email)).Should().Contain(
            CandidateRoleAssignment.RoleName,
            "o login Google numa conta existente deve garantir a role, não apenas na criação");
    }

    [Fact]
    public async Task Handle_SegundoLoginGoogle_DeveReutilizarAContaEManterRole()
    {
        var email = TestDataFactory.UniqueEmail("gg_role_segundo");
        var subject = $"sub-{Guid.NewGuid():N}";
        SetupGoogle("tok-1", subject, email);

        await LoginAsync("tok-1");

        long idPrimeiroLogin;
        await using (var scope = _fx.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            idPrimeiroLogin = (await users.FindByEmailAsync(email))!.Id;
        }

        // Mesmo subject: deve entrar pelo caminho do login externo, sem criar outra conta.
        SetupGoogle("tok-2", subject, email);
        await LoginAsync("tok-2");

        await using (var scope = _fx.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await users.FindByEmailAsync(email);
            user!.Id.Should().Be(idPrimeiroLogin, "o segundo login não deve criar uma conta nova");
            (await users.GetRolesAsync(user)).Should().Contain(CandidateRoleAssignment.RoleName);
        }
    }

    [Fact]
    public async Task Handle_ContaEliminada_DeveRecusarLogin()
    {
        var email = TestDataFactory.UniqueEmail("gg_role_eliminada");
        var subject = $"sub-{Guid.NewGuid():N}";
        SetupGoogle("tok-del", subject, email);

        await LoginAsync("tok-del");

        await using (var scope = _fx.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await users.FindByEmailAsync(email);
            user!.IsDeleted = true;
            (await users.UpdateAsync(user)).Succeeded.Should().BeTrue();
        }

        SetupGoogle("tok-del-2", subject, email);

        var act = async () => await LoginAsync("tok-del-2");

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
    }

    private sealed record UserLoggedViewModelProbe(string Email, string AccessToken, string? RefreshToken);
}

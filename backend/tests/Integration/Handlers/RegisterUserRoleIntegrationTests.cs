using EmpregaNet.Application.Auth.Commands;
using EmpregaNet.Application.Users.Identity;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Objetivo: garantir que o registo por senha deixa o utilizador com a role de candidato.
/// </summary>
/// <remarks>
/// O registo partilha o <see cref="CandidateRoleAssignment"/> com o login Google e sofria do mesmo
/// problema: o resultado da atribuição era descartado, e um utilizador podia terminar o registo
/// sem role nenhuma sem que nada sinalizasse.
/// </remarks>
[Collection("Integration")]
public sealed class RegisterUserRoleIntegrationTests
{
    private readonly InMemoryIdentityFixture _fx;

    public RegisterUserRoleIntegrationTests(InMemoryIdentityFixture fx) => _fx = fx;

    [Fact]
    public async Task Handle_RegistoValido_DeveTerminarComRoleCandidate()
    {
        var email = TestDataFactory.UniqueEmail("reg_role");
        var username = TestDataFactory.UniqueUsername("reg_role");

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();

        var id = await sut.Handle(
            new RegisterUserCommand(
                username,
                email,
                AuthIntegrationTestHelper.DefaultPassword,
                AuthIntegrationTestHelper.DefaultPassword,
                TestDataFactory.UniqueBrazilianCell()),
            CancellationToken.None);

        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await users.FindByIdAsync(id.ToString());

        user.Should().NotBeNull();
        (await users.GetRolesAsync(user!)).Should().Contain(CandidateRoleAssignment.RoleName);
    }
}

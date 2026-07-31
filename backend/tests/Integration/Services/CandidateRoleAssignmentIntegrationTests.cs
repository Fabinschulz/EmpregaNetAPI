using EmpregaNet.Application.Users.Identity;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Tests.Integration.Services;

/// <summary>
/// Objetivo: garantir que a atribuição da role de candidato devolve o resultado da operação e é
/// idempotente.
/// </summary>
/// <remarks>
/// Os <see cref="IdentityResult"/> de <c>CreateAsync</c> e <c>AddToRoleAsync</c> eram descartados,
/// então uma falha passava em silêncio e quem chamava não tinha como saber. Estes testes fixam o
/// contrato de devolver o resultado.
/// </remarks>
[Collection("Integration")]
public sealed class CandidateRoleAssignmentIntegrationTests
{
    private readonly InMemoryIdentityFixture _fx;

    public CandidateRoleAssignmentIntegrationTests(InMemoryIdentityFixture fx) => _fx = fx;

    private async Task<User> CreateUserAsync(string prefix)
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = new User
        {
            UserName = TestDataFactory.UniqueUsername(prefix),
            Email = TestDataFactory.UniqueEmail(prefix),
            EmailConfirmed = true,
            UserType = UserTypeEnum.Candidate
        };

        (await users.CreateAsync(user)).Succeeded.Should().BeTrue();
        return user;
    }

    [Fact]
    public async Task EnsureCandidateRoleAsync_UtilizadorSemRole_DeveAtribuirEDevolverSucesso()
    {
        var user = await CreateUserAsync("role_atribui");

        await using var scope = _fx.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        var tracked = await users.FindByIdAsync(user.Id.ToString());

        var result = await CandidateRoleAssignment.EnsureCandidateRoleAsync(tracked!, users, roles);

        result.Succeeded.Should().BeTrue();
        (await users.GetRolesAsync(tracked!)).Should().Contain(CandidateRoleAssignment.RoleName);
    }

    [Fact]
    public async Task EnsureCandidateRoleAsync_ChamadaDuasVezes_DeveSerIdempotente()
    {
        var user = await CreateUserAsync("role_idem");

        await using var scope = _fx.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        var tracked = await users.FindByIdAsync(user.Id.ToString());

        var first = await CandidateRoleAssignment.EnsureCandidateRoleAsync(tracked!, users, roles);
        var second = await CandidateRoleAssignment.EnsureCandidateRoleAsync(tracked!, users, roles);

        first.Succeeded.Should().BeTrue();
        second.Succeeded.Should().BeTrue("a segunda chamada deve devolver sucesso sem duplicar a role");

        var assigned = await users.GetRolesAsync(tracked!);
        assigned.Count(r => r == CandidateRoleAssignment.RoleName).Should().Be(1);
    }
}

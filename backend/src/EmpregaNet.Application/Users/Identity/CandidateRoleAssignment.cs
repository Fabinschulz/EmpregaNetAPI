using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Application.Users.Identity;

/// <summary>
/// Garante a role padrão de candidato (alinhado ao registo por e-mail/senha).
/// </summary>
public static class CandidateRoleAssignment
{
    public const string RoleName = "Candidate";

    public static async Task EnsureCandidateRoleAsync(
        User user,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        if (await userManager.IsInRoleAsync(user, RoleName))
            return;

        if (!await roleManager.RoleExistsAsync(RoleName))
            await roleManager.CreateAsync(new Role { Name = RoleName });

        await userManager.AddToRoleAsync(user, RoleName);
    }
}

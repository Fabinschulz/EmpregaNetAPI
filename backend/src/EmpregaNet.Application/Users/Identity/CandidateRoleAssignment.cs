using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Application.Users.Identity;

/// <summary>
/// Garante a role padrão de candidato (alinhado ao registo por e-mail/senha).
/// </summary>
public static class CandidateRoleAssignment
{
    public const string RoleName = "Candidate";

    /// <summary>
    /// Garante que o operador tem a role de candidato.
    /// </summary>
    /// <returns>
    /// O resultado da operação. <b>Devolve</b> em vez de ignorar: antes os
    /// <see cref="IdentityResult"/> de <c>CreateAsync</c> e <c>AddToRoleAsync</c> eram
    /// descartados, então uma falha ao associar a role passava em silêncio e o operador ficava
    /// sem role nenhuma, sem qualquer sinal para quem chamou.
    /// </returns>
    public static async Task<IdentityResult> EnsureCandidateRoleAsync(
        User user,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        if (await userManager.IsInRoleAsync(user, RoleName))
            return IdentityResult.Success;

        if (!await roleManager.RoleExistsAsync(RoleName))
        {
            var createRole = await roleManager.CreateAsync(new Role { Name = RoleName });
            if (!createRole.Succeeded)
                return createRole;
        }

        return await userManager.AddToRoleAsync(user, RoleName);
    }
}

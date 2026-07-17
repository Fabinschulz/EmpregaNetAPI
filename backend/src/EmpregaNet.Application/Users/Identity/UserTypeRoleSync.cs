using EmpregaNet.Application.Auth;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Application.Users.Identity;

/// <summary>
/// Mantém roles Identity alinhadas ao <see cref="User.UserType"/> do domínio.
/// </summary>
public static class UserTypeRoleSync
{
    private static readonly string[] ManagedRoles =
    [
        RecruitmentRoleNames.Admin,
        RecruitmentRoleNames.Recruiter,
        RecruitmentRoleNames.Manager,
        CandidateRoleAssignment.RoleName,
    ];

    public static IReadOnlyList<string> RolesForUserType(UserTypeEnum userType) => userType switch
    {
        UserTypeEnum.Admin => [RecruitmentRoleNames.Admin],
        UserTypeEnum.Recruiter => [RecruitmentRoleNames.Recruiter],
        UserTypeEnum.Manager => [RecruitmentRoleNames.Manager],
        UserTypeEnum.Candidate => [CandidateRoleAssignment.RoleName],
        _ => []
    };

    public static async Task SyncRolesAsync(
        User user,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var targetRoles = RolesForUserType(user.UserType).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var currentRoles = await userManager.GetRolesAsync(user);

        foreach (var role in currentRoles.Where(r => ManagedRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
        {
            if (!targetRoles.Contains(role))
            {
                await userManager.RemoveFromRoleAsync(user, role);
            }
        }

        foreach (var role in targetRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new Role { Name = role });
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}

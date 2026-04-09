using System.Security.Claims;

namespace EmpregaNet.Application.Auth;

/// <summary>
/// Perfis que podem gerenciar vagas, empresas (conforme policy) e pipeline de candidaturas.
/// Alinhado às policies em <c>IdentityConfig</c> (<see cref="Utils.Constants.AuthPolicies"/>).
/// </summary>
public static class RecruitmentRoleNames
{
    public const string Admin = "Admin";
    public const string Recruiter = "Recruiter";
    public const string Manager = "Manager";

    /// <summary>Equipe de recrutamento (não inclui candidato nem papéis auxiliares como entrevistador).</summary>
    public static readonly string[] Staff = [Admin, Recruiter, Manager];

    public static bool IsRecruitmentStaff(IReadOnlyCollection<string> roleNames)
    {
        if (roleNames.Count == 0)
            return false;

        return roleNames.Any(r =>
            Staff.Any(s => s.Equals(r, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>Usa o principal HTTP (JWT) para decidir se vê catálogo completo de vagas vs. apenas publicadas.</summary>
    public static bool IsRecruitmentStaff(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        return Staff.Any(role => user.IsInRole(role));
    }
}

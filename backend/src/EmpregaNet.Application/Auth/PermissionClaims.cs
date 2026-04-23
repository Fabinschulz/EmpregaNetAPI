namespace EmpregaNet.Application.Auth;

/// <summary>
/// Contrato de claims de permissão: JWT compacto (<see cref="JwtScopes"/>) e persistência em roles (Identity).
/// </summary>
public static class PermissionClaims
{
    /// <summary>Claim no JWT: todos os escopos do usuário, separados por espaço (ex.: <c>user:read job:update</c>).</summary>
    public const string JwtScopes = "scopes";

    /// <summary>Tipo de claim em <c>AspNetRoleClaims</c> por permissão associada à role.</summary>
    public const string IdentityRolePermission = "permission";
}

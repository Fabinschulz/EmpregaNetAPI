using System.Security.Claims;
using EmpregaNet.Application.Auth.ViewModel;

namespace EmpregaNet.Application.Auth;

public static class PermissionClaimParser
{
    public static List<UserPermissionVieModel> ParseFromClaims(IEnumerable<Claim> claims)
    {
        var list = claims as IList<Claim> ?? claims.ToList();
        var scopes = list.FirstOrDefault(c => c.Type == PermissionClaims.JwtScopes)?.Value;
        if (string.IsNullOrWhiteSpace(scopes))
            return [];

        return scopes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(UserPermissionVieModel.FromStoredPair)
            .ToList();
    }
}

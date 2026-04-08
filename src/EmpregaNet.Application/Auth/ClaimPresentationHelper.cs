using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EmpregaNet.Application.Auth;

/// <summary>
/// Normaliza tipos de claim verbosos do Identity/JWT para nomes curtos na resposta da API.
/// </summary>
public static class ClaimPresentationHelper
{
    public static string ToShortType(string claimType) => claimType switch
    {
        ClaimTypes.Role => "role",
        ClaimTypes.NameIdentifier => "nameIdentifier",
        var t when t == JwtRegisteredClaimNames.Sub => "sub",
        var t when t == JwtRegisteredClaimNames.Email => "email",
        var t when t == JwtRegisteredClaimNames.Jti => "jti",
        var t when t == JwtRegisteredClaimNames.Iat => "iat",
        _ => claimType
    };
}

using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Utils;

namespace EmpregaNet.Api.Configuration;

public sealed class AuthCookieService(IConfiguration configuration, IHostEnvironment environment)
{
    public void AppendLoginCookies(HttpResponse response, UserLoggedViewModel login)
    {
        var accessMinutes = configuration.GetValue("JwtSettings:ExpirationHours", 8) * 60;
        var refreshDays = configuration.GetValue("JwtSettings:RefreshTokenExpirationDays", 14);

        var access = StripBearer(login.AccessToken);
        Append(response, Constants.AuthCookies.AccessToken, access, TimeSpan.FromMinutes(accessMinutes));

        if (!string.IsNullOrWhiteSpace(login.RefreshToken))
        {
            Append(response, Constants.AuthCookies.RefreshToken, login.RefreshToken!, TimeSpan.FromDays(refreshDays));
        }
    }

    /// <summary>
    /// Remove os cookies de autenticação.
    /// </summary>
    /// <remarks>
    /// A remoção precisa repetir os mesmos atributos usados na emissão. O navegador identifica
    /// um cookie por nome + domínio + caminho; divergir nesses atributos faz o
    /// <c>Set-Cookie</c> de expiração criar/apagar um cookie diferente e deixar o original
    /// intacto, ou seja, um logout que não desloga.
    /// </remarks>
    public void ClearLoginCookies(HttpResponse response)
    {
        var options = BuildCookieOptions(null);

        response.Cookies.Delete(Constants.AuthCookies.AccessToken, options);
        response.Cookies.Delete(Constants.AuthCookies.RefreshToken, options);
    }

    private void Append(HttpResponse response, string name, string value, TimeSpan maxAge)
        => response.Cookies.Append(name, value, BuildCookieOptions(maxAge));

    private CookieOptions BuildCookieOptions(TimeSpan? maxAge) => new()
    {
        HttpOnly = true,
        Secure = !environment.IsDevelopment(),
        SameSite = SameSiteMode.Lax,
        Path = "/",
        MaxAge = maxAge
    };

    private static string StripBearer(string token) =>
        token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? token[7..] : token;
}

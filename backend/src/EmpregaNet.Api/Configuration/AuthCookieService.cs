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

    public void ClearLoginCookies(HttpResponse response)
    {
        response.Cookies.Delete(Constants.AuthCookies.AccessToken);
        response.Cookies.Delete(Constants.AuthCookies.RefreshToken);
    }

    private void Append(HttpResponse response, string name, string value, TimeSpan maxAge)
    {
        response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = maxAge
        });
    }

    private static string StripBearer(string token) =>
        token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? token[7..] : token;
}

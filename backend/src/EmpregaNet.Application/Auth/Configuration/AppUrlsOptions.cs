namespace EmpregaNet.Application.Auth.Configuration;

/// <summary>
/// URLs públicas usadas em e-mails transacionais (redefinição de senha, etc.).
/// </summary>
public sealed class AppUrlsOptions
{
    public const string SectionName = "AppUrls";

    /// <summary>URL base do front-end, sem barra final (ex.: https://app.exemplo.com).</summary>
    public string PublicAppBaseUrl { get; set; } = "https://localhost:3000";

    /// <summary>Caminho da página de nova senha (ex.: /auth/reset-password).</summary>
    public string PasswordResetPath { get; set; } = "/auth/reset-password";

    /// <summary>Caminho da página que confirma o e-mail (ex.: /auth/confirm-email).</summary>
    public string EmailConfirmationPath { get; set; } = "/auth/confirm-email";

    /// <summary>Origens permitidas no CORS (front-end que consome a API).</summary>
    public string[] CorsAllowedOrigins { get; set; } = ["http://localhost:3000", "https://localhost:3000"];

    /// <summary>
    /// Origens efetivas para a policy CORS: <see cref="CorsAllowedOrigins"/> ou, se vazio, <see cref="PublicAppBaseUrl"/>.
    /// </summary>
    public string[] ResolveCorsOrigins()
    {
        var fromList = CorsAllowedOrigins?
            .Where(static o => !string.IsNullOrWhiteSpace(o))
            .Select(static o => o.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (fromList is { Length: > 0 })
            return fromList;

        if (string.IsNullOrWhiteSpace(PublicAppBaseUrl))
            return ["http://localhost:3000", "https://localhost:3000"];

        return [PublicAppBaseUrl.Trim().TrimEnd('/')];
    }
}

namespace EmpregaNet.Application.Interfaces;

/// <summary>
/// Valida um <c>id_token</c> JWT do Google e expõe claims mínimas para login na API.
/// </summary>
public interface IGoogleIdTokenValidator
{
    /// <returns>Informação do utilizador ou <c>null</c> se o token for inválido ou a integração desconfigurada.</returns>
    Task<GoogleIdTokenPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}

/// <summary>Dados extraídos do token Google após validação criptográfica.</summary>
public sealed record GoogleIdTokenPayload(string Subject, string Email, bool EmailVerified);

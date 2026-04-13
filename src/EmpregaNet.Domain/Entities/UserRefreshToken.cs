namespace EmpregaNet.Domain.Entities;

/// <summary>
/// Refresh token opaco (hash armazenado). Rotação ao renovar o access token.
/// </summary>
public sealed class UserRefreshToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>SHA-256 em hexadecimal (64 caracteres) do token enviado ao cliente.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}

using System.Security.Cryptography;
using System.Text;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly PostgreSqlContext _db;
    private readonly JwtSettings _jwt;

    public RefreshTokenService(PostgreSqlContext db, IOptions<JwtSettings> jwtOptions)
    {
        _db = db;
        _jwt = jwtOptions.Value;
    }

    public async Task<string> IssueAsync(long userId, CancellationToken cancellationToken = default)
    {
        var plain = CreateOpaqueToken();
        var entity = new UserRefreshToken
        {
            UserId = userId,
            TokenHash = HashToken(plain),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(Math.Max(1, _jwt.RefreshTokenExpirationDays))
        };

        _db.UserRefreshTokens.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return plain;
    }

    public async Task<(User User, string NewRefreshToken)?> RotateAsync(string plainRefreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plainRefreshToken))
            return null;

        var hash = HashToken(plainRefreshToken.Trim());
        var row = await _db.UserRefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);

        if (row is null || row.RevokedAt is not null || row.ExpiresAt < DateTimeOffset.UtcNow)
            return null;

        if (row.User.IsDeleted)
            return null;

        row.RevokedAt = DateTimeOffset.UtcNow;

        var newPlain = CreateOpaqueToken();
        _db.UserRefreshTokens.Add(new UserRefreshToken
        {
            UserId = row.UserId,
            TokenHash = HashToken(newPlain),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(Math.Max(1, _jwt.RefreshTokenExpirationDays))
        });

        await _db.SaveChangesAsync(cancellationToken);
        return (row.User, newPlain);
    }

    public async Task RevokeAllForUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var rows = await _db.UserRefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt >= now)
            .ToListAsync(cancellationToken);

        foreach (var row in rows)
            row.RevokedAt = now;

        if (rows.Count > 0)
            await _db.SaveChangesAsync(cancellationToken);
    }

    private static string CreateOpaqueToken()
    {
        Span<byte> data = stackalloc byte[48];
        RandomNumberGenerator.Fill(data);
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

using EmpregaNet.Domain.Entities;

namespace EmpregaNet.Application.Interfaces;

/// <summary>
/// Emissão, rotação e revogação de refresh tokens opacos (persistidos por hash).
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Gera e persiste um refresh token; devolve o valor para o cliente.</summary>
    Task<string> IssueAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida o token, revoga a linha atual, cria um novo refresh token e devolve o usuário e o novo token para o cliente.
    /// Se o token for inválido ou revogado, devolve null (cliente deve forçar o usuário a fazer login novamente).
    /// </summary>
    Task<(User User, string NewRefreshToken)?> RotateAsync(string plainRefreshToken, CancellationToken cancellationToken = default);

    /// <summary>Revoga todos os refresh tokens ativos do utilizador (ex.: após mudança de senha).</summary>
    Task RevokeAllForUserAsync(long userId, CancellationToken cancellationToken = default);
}

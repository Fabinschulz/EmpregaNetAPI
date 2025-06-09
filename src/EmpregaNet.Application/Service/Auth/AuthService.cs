using EmpregaNet.Application.ViewModel;
using EmpregaNet.Infra.Cache;
using EmpregaNet.Infra.Cache.MemoryService;
using EmpregaNet.Infra.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmpregaNet.Application.Service;

/// <summary>
/// Serviço responsável pela autenticação de usuários, geração de tokens JWT e gerenciamento de permissões.
/// Realiza a validação de credenciais, criação de claims, emissão de tokens e consulta de permissões do usuário.
/// </summary>
public class AuthService
{
    /// <summary>
    /// Configurações de autenticação (chave secreta, emissor, público, tempo de expiração).
    /// </summary>
    private readonly JwtSettings _authSettings;

    /// <summary>
    /// Serviço de cache em memória utilizado para armazenar e recuperar permissões do usuário.
    /// </summary>
    private readonly IMemoryService _memoryService;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="AuthService"/>.
    /// </summary>
    /// <param name="authSettings">Configurações de autenticação injetadas via <see cref="IOptions{AuthSettings}"/>.</param>
    /// <param name="memoryService">Serviço de cache em memória.</param>
    public AuthService(
        IOptions<JwtSettings> authSettings, IMemoryService memoryService)
    {
        _authSettings = authSettings.Value;
        _memoryService = memoryService;
    }

    /// <summary>
    /// Recupera todas as permissões do usuário autenticado a partir do cache.
    /// </summary>
    /// <param name="key">Chave única do usuário.</param>
    /// <returns>Lista de permissões (<see cref="UserPermission"/>) ou <c>null</c> se não houver permissões.</returns>
    public async Task<List<UserPermissionVieModel>?> GetAllPermissions(string key)
    {
        var tokenPermission = $"{key}:{CacheKeyType.Permissions}";

        var permissions = await _memoryService.GetValueAsync<List<UserPermissionVieModel>>(tokenPermission);

        return permissions;
    }


    /// <summary>
    /// Gera o token JWT a partir dos claims de identidade do usuário.
    /// </summary>
    /// <param name="identityClaims">Claims de identidade do usuário.</param>
    /// <returns>Token JWT codificado como string, com prefixo "Bearer".</returns>
    private string CodifyingToken(ClaimsIdentity identityClaims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_authSettings.SecretKey);
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _authSettings.Issuer,
            Audience = _authSettings.Audience,
            Subject = identityClaims,
            Expires = DateTime.UtcNow.AddHours(_authSettings.ExpirationHours),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        });

        var access = $"Bearer {tokenHandler.WriteToken(token)}";

        return access;
    }


    /// <summary>
    /// Converte uma data para o formato Unix Epoch (segundos desde 01/01/1970).
    /// </summary>
    /// <param name="date">Data a ser convertida.</param>
    /// <returns>Valor em segundos desde o Unix Epoch.</returns>
    private static long ToUnixEpochDate(DateTime date) => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
}

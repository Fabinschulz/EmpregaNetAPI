using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtBuilder : IJwtBuilder
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly JwtSettings _jwtSettings;
    private readonly IMemoryService _memoryService;

    public JwtBuilder(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IOptions<JwtSettings> jwtSettingsOptions,
        IMemoryService memoryService)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _jwtSettings = jwtSettingsOptions.Value ?? throw new ArgumentNullException(nameof(jwtSettingsOptions));
        _memoryService = memoryService ?? throw new ArgumentNullException(nameof(memoryService));
    }

    /// <summary>
    /// Constrói um token JWT para o usuário autenticado.
    /// </summary>
    /// <param name="user">Usuário autenticado para o qual o token será gerado.</param>
    /// <returns>Um objeto <see cref="UserLoggedViewModel"/> contendo o token e informações do usuário.</returns>
    /// /// <exception cref="ArgumentNullException">Lançada se o usuário for nulo.</exception>  
    /// <remarks>
    /// Este método cria um token JWT contendo as claims do usuário, incluindo permissões e roles.
    /// </remarks>

    public async Task<UserLoggedViewModel> BuildUserTokenAsync(User user)
    {

        var claimsIdentity = await BuildClaimsIdentityAsync(user);
        var token = GenerateToken(claimsIdentity);

        return new UserLoggedViewModel
        {
            AccessToken = token,
            ExpiresIn = TimeSpan.FromHours(_jwtSettings.ExpirationHours).TotalSeconds,
            UserToken = new UserToken
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Claims = claimsIdentity.Claims
                    .Select(c => new UserClaim { Type = c.Type, Value = c.Value })
                    .ToList()
            },
            Permissions = claimsIdentity.Claims
                .Where(c => c.Type == "permission")
                .Select(c => new UserPermissionVieModel
                {
                    Resource = Enum.Parse<PermissionResourceEnum>(c.Value.Split(':')[0]),
                    Type = Enum.Parse<PermissionTypeEnum>(c.Value.Split(':')[1])
                })
                .ToList()

        };
    }

    private async Task<ClaimsIdentity> BuildClaimsIdentityAsync(User user)
    {
        var identity = new ClaimsIdentity();
        identity.AddClaims(GetBasicJwtClaims(user));

        var userClaims = await _userManager.GetClaimsAsync(user);
        identity.AddClaims(userClaims);
        await AddRoleClaimsAsync(user, identity);
        await AddPermissionClaimsFromRolesAsync(user, identity);

        return identity;
    }

    private IEnumerable<Claim> GetBasicJwtClaims(User user)
    {
        return new[]
        {
                new Claim("userId", user.Id.ToString()),
                new Claim("userName", user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64),
            };
    }

    private async Task AddRoleClaimsAsync(User user, ClaimsIdentity identity)
    {
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var roleName in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
        }
    }

    /// <summary>
    /// Recupera todas as permissões do usuário autenticado a partir do cache.
    /// </summary>
    /// <param name="key">Chave única do usuário.</param>
    /// <returns>Lista de permissões (<see cref="UserPermissionVieModel"/>) ou <c>null</c> se não houver permissões.</returns>
    public async Task<List<UserPermissionVieModel>?> GetAllPermissions(string key)
    {
        var tokenPermission = $"{key}:{CacheKeyType.Permissions}";
        var permissions = await _memoryService.GetValueAsync<List<UserPermissionVieModel>>(tokenPermission);

        return permissions;
    }

    /// <summary>
    /// Recupera todas as permissões do usuário autenticado a partir do cache.
    /// </summary>
    /// <param name="key">Chave única do usuário.</param>
    /// <returns>Lista de permissões (<see cref="UserPermissionVieModel"/>) ou <c>null</c> se não houver permissões.</returns>
    public async Task<List<UserPermissionVieModel>?> GetAllPermissionsAsync(string key)
    {
        var tokenPermission = $"{key}:{CacheKeyType.Permissions}";

        var permissions = await _memoryService.GetValueAsync<List<UserPermissionVieModel>>(tokenPermission);
        return permissions;
    }

    private async Task AddPermissionClaimsFromRolesAsync(User user, ClaimsIdentity identity)
    {
        var userRoleNames = await _userManager.GetRolesAsync(user);

        foreach (var roleName in userRoleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                identity.AddClaims(roleClaims.Where(c => c.Type == "permission"));
            }
        }
    }

    /// <summary>
    /// Gera o token JWT a partir dos claims de identidade do usuário.
    /// </summary>
    /// <param name="claimsIdentity">Claims de identidade do usuário.</param>
    /// <returns>Token JWT codificado como string, com prefixo "Bearer".</returns>
    private string GenerateToken(ClaimsIdentity claimsIdentity)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return $"Bearer {tokenHandler.WriteToken(token)}";
    }

    /// <summary>
    /// Converte uma data para o formato Unix Epoch (segundos desde 01/01/1970).
    /// </summary>
    /// <param name="date">Data a ser convertida.</param>
    /// <returns>Valor em segundos desde o Unix Epoch.</returns>
    private static long ToUnixEpochDate(DateTime date)
        => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
            .TotalSeconds);
}
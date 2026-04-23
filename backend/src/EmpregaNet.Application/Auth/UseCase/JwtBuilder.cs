using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Domain.Entities;
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

    public JwtBuilder(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IOptions<JwtSettings> jwtSettingsOptions)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _jwtSettings = jwtSettingsOptions.Value ?? throw new ArgumentNullException(nameof(jwtSettingsOptions));
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
        var key = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Id.ToString()));

        var claimsIdentity = await BuildClaimsIdentityAsync(user);
        var token = GenerateToken(claimsIdentity);

        var permissionModels = PermissionClaimParser.ParseFromClaims(claimsIdentity.Claims);
        var roles = claimsIdentity.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var clientClaims = claimsIdentity.Claims.Where(c =>
            c.Type != PermissionClaims.JwtScopes
            && c.Type != ClaimTypes.Role);

        return new UserLoggedViewModel
        {
            AccessToken = token,
            ExpiresIn = TimeSpan.FromHours(_jwtSettings.ExpirationHours).TotalSeconds,
            UserToken = new UserToken
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles,
                Claims = clientClaims
                    .Select(c => new UserClaim { Type = ClaimPresentationHelper.ToShortType(c.Type), Value = c.Value })
                    .ToList()
            },
            Permissions = permissionModels,
            Key = key
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
        var key = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Id.ToString()));
        return new[]
        {
                new Claim("userId", user.Id.ToString()),
                new Claim("userName", user.UserName ?? string.Empty),
                new Claim("key", key),
                new Claim(ClaimTypes.NameIdentifier, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTimeOffset.UtcNow).ToString(), ClaimValueTypes.Integer64),
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

    private async Task AddPermissionClaimsFromRolesAsync(User user, ClaimsIdentity identity)
    {
        var userRoleNames = await _userManager.GetRolesAsync(user);
        var codes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var roleName in userRoleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
                continue;

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var c in roleClaims.Where(c => c.Type == PermissionClaims.IdentityRolePermission))
            {
                try
                {
                    codes.Add(UserPermissionVieModel.FromStoredPair(c.Value).Code);
                }
                catch (FormatException)
                {
                    // claim malformada no banco — ignora
                }
            }
        }

        if (codes.Count == 0)
            return;

        var joined = string.Join(' ', codes.OrderBy(x => x, StringComparer.Ordinal));
        identity.AddClaim(new Claim(PermissionClaims.JwtScopes, joined));
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
            Expires = DateTimeOffset.UtcNow.AddHours(_jwtSettings.ExpirationHours).UtcDateTime,
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
    private static long ToUnixEpochDate(DateTimeOffset date)
        => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
            .TotalSeconds);
}
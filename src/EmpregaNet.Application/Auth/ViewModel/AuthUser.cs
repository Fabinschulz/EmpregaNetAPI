using Newtonsoft.Json;

namespace EmpregaNet.Application.Auth.ViewModel;

public class UserRegistry
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PasswordConfirmation { get; set; }
}

public class UserLoggedViewModel
{
    public required string AccessToken { get; set; }
    public required double ExpiresIn { get; set; }
    public required UserToken UserToken { get; set; }

    /// <summary>Lista interna (enums + chaves); não é serializada no JSON.</summary>
    [JsonIgnore]
    public List<UserPermissionVieModel>? Permissions { get; set; }

    /// <summary>Permissões agrupadas por recurso, prontas para menus e políticas de UI.</summary>
    [JsonProperty("permissions")]
    public IReadOnlyDictionary<string, IReadOnlyList<string>> PermissionsByResource =>
        Permissions is null || Permissions.Count == 0
            ? new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
            : Permissions
                .GroupBy(p => p.ResourceKey, StringComparer.Ordinal)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<string>)g.Select(x => x.ActionKey)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(x => x, StringComparer.Ordinal)
                        .ToList(),
                    StringComparer.Ordinal);

    /// <summary>Lista única de códigos <c>recurso:ação</c> para checagens rápidas no cliente.</summary>
    [JsonProperty("permissionCodes")]
    public IReadOnlyList<string> PermissionCodes =>
        Permissions is null || Permissions.Count == 0
            ? Array.Empty<string>()
            : Permissions.Select(p => p.Code).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();

    public string Key { get; set; } = string.Empty;

    public UserLoggedViewModel()
    {
    }
}

public class UserToken
{
    public required long Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }

    [JsonProperty("roles")]
    public List<string> Roles { get; set; } = [];

    /// <summary>Claims essenciais (sem permissões nem roles duplicadas).</summary>
    public required IEnumerable<UserClaim> Claims { get; set; }

    /// <summary>Perfis do usuário para regras de negócio (preenchido a partir das roles do JWT).</summary>
    public HashSet<string> GetRoleNames() => Roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
}

public class UserClaim
{
    public required string Value { get; set; }
    public required string Type { get; set; }
}

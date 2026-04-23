using EmpregaNet.Domain.Enums;
using Newtonsoft.Json;

namespace EmpregaNet.Application.Auth.ViewModel;

/// <summary>
/// Permissão exposta na API em formato legível (sem enums numéricos no JSON).
/// </summary>
public sealed class UserPermissionVieModel
{
    /// <summary>Recurso em minúsculas, alinhado ao nome do enum (ex.: <c>company</c>, <c>application</c>).</summary>
    [JsonProperty("resource")]
    public string ResourceKey { get; init; } = string.Empty;

    /// <summary>Ação em minúsculas: <c>read</c>, <c>create</c>, <c>update</c>, <c>delete</c>.</summary>
    [JsonProperty("action")]
    public string ActionKey { get; init; } = string.Empty;

    /// <summary>Código canônico <c>recurso:ação</c> para checagens no cliente.</summary>
    [JsonProperty("code")]
    public string Code { get; init; } = string.Empty;

    [JsonIgnore]
    public PermissionResourceEnum Resource { get; init; }

    [JsonIgnore]
    public PermissionTypeEnum Type { get; init; }

    public static UserPermissionVieModel FromStoredPair(string resourceAndType)
    {
        var parts = resourceAndType.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            throw new FormatException($"Permissão inválida: '{resourceAndType}'.");

        var resource = Enum.Parse<PermissionResourceEnum>(parts[0], ignoreCase: true);
        var type = Enum.Parse<PermissionTypeEnum>(parts[1], ignoreCase: true);
        return FromEnums(resource, type);
    }

    public static UserPermissionVieModel FromEnums(PermissionResourceEnum resource, PermissionTypeEnum type)
    {
        var rk = resource.ToString().ToLowerInvariant();
        var ak = type.ToString().ToLowerInvariant();
        return new UserPermissionVieModel
        {
            Resource = resource,
            Type = type,
            ResourceKey = rk,
            ActionKey = ak,
            Code = $"{rk}:{ak}"
        };
    }
}

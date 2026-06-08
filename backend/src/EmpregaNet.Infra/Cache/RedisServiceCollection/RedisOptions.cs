using Microsoft.Extensions.Configuration;

namespace EmpregaNet.Infra.Cache;

/// <summary>
/// Redis opcional: desligado por padrão; habilite <see cref="Enabled"/> quando tiver instância disponível.
/// </summary>
public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public bool Enabled { get; set; }

    public string ConnectionString { get; set; } = string.Empty;

    public string InstanceName { get; set; } = "EmpregaNet:";

    public bool IsActive => Enabled && !string.IsNullOrWhiteSpace(ConnectionString);

    public static RedisOptions Resolve(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);

        if (!string.IsNullOrEmpty(section["ConnectionString"]) || section.GetValue<bool?>("Enabled") is not null)
            return section.Get<RedisOptions>() ?? new RedisOptions();

        var legacyConnection = section.Value;
        if (!string.IsNullOrWhiteSpace(legacyConnection))
        {
            return new RedisOptions
            {
                ConnectionString = legacyConnection,
                Enabled = false
            };
        }

        return new RedisOptions();
    }
}

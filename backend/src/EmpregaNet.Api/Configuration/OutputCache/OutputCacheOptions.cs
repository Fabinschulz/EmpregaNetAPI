namespace EmpregaNet.Api.Configuration;

public sealed class OutputCacheOptions
{
    public const string SectionName = "OutputCache";

    /// <summary>
    /// Tempo padrão de expiração do cache (em minutos). Ignorado quando Redis store está ativo.
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 90;

    /// <summary>Limite total do cache in-process (MB). Ignorado quando Redis store está ativo.</summary>
    public int SizeLimitMegabytes { get; set; } = 100;

    /// <summary>Respostas acima deste tamanho não são cacheadas (MB). Default Microsoft: 100.</summary>
    public int MaximumBodySizeMegabytes { get; set; } = 100;
}

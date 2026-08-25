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

    /// <summary>
    /// Expiração das leituras do dashboard, em segundos.
    /// </summary>
    /// <remarks>
    /// Cinco minutos equilibra as duas pressões: cada resposta custa uma dezena de agregações, e o
    /// cabeçalho da tela mostra ao utilizador a idade do dado. Mais que isso e o "última atualização"
    /// fica velho a ponto de o botão de atualizar parecer quebrado; menos e o cache deixa de aliviar
    /// o banco nos horários de pico.
    /// </remarks>
    public int DashboardExpirationSeconds { get; set; } = 300;
}

namespace EmpregaNet.AI.Configuration;

/// <summary>Configuração OpenAI (API key via User Secrets ou variável de ambiente).</summary>
public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public bool Enabled { get; set; }

    /// <summary>Chave da API. Preferir User Secrets / env <c>OPENAI__APIKEY</c>.</summary>
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gpt-4o-mini";

    public string? OrganizationId { get; set; }

    public int MaxCompletionTokens { get; set; } = 4096;

    public double Temperature { get; set; } = 0.2;

    public bool UseForResumeParsing { get; set; } = true;

    public bool UseForJobMatching { get; set; } = true;

    /// <summary>Se OpenAI falhar ou estiver desligado, usa heurística/scorer local.</summary>
    public bool FallbackToHeuristics { get; set; } = true;

    /// <summary>Máximo de vagas enviadas ao modelo no matching (resto: scorer determinístico).</summary>
    public int JobMatchingMaxJobsForLlm { get; set; } = 30;
}

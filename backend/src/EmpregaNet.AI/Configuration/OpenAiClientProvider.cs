using EmpregaNet.AI.Configuration;
using Microsoft.Extensions.Options;
using OpenAI;

namespace EmpregaNet.AI.Services.OpenAi;

/// <summary>
/// Equivalente DI de <c>new OpenAIClient(Configuration.OpenAI.ApiKey)</c> — cliente reutilizado por request scope da app.
/// </summary>
public sealed class OpenAiClientProvider(IOptions<OpenAiOptions> options) : IOpenAiClientProvider
{
    private readonly OpenAiOptions _options = options.Value;
    private OpenAIClient? _client;

    public bool IsConfigured =>
        _options.Enabled && !string.IsNullOrWhiteSpace(_options.ApiKey);

    public OpenAIClient GetClient()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "OpenAI não configurado. Defina OpenAI:Enabled=true e OpenAI:ApiKey (User Secrets ou env OPENAI__APIKEY).");
        }

        return _client ??= new OpenAIClient(_options.ApiKey);
    }
}

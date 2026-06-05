using OpenAI;

namespace EmpregaNet.AI.Services.OpenAi;

/// <summary>Fornece instância única de <see cref="OpenAIClient"/> a partir de <see cref="Configuration.OpenAiOptions"/>.</summary>
public interface IOpenAiClientProvider
{
    bool IsConfigured { get; }

    OpenAIClient GetClient();
}

using EmpregaNet.AI.Configuration;
using EmpregaNet.AI.Services.OpenAi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.AI;

public static class DependencyInjection
{
    public static IServiceCollection RegisterAIDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        RegisterOpenAi(services, configuration);

        return services;
    }

    private static void RegisterOpenAi(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.AddSingleton<IOpenAiClientProvider, OpenAiClientProvider>();
    }

}

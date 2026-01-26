using Bff.Core.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bff.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        var config = configuration.GetSection(EmpregaNetApi.SectionName).Get<EmpregaNetApi>();
        if (config is null || string.IsNullOrEmpty(config.BaseUrl))
        {
            Console.WriteLine("Aviso: Configuração 'EmpregaNetApi' não encontrada ou incompleta. Usando provedores de log padrão.");
            return services;
        }

        services.AddHttpApiClient(config.BaseUrl);
        return services;
    }
}
using Bff.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bff.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        var EmpregaNetApi = configuration["EmpregaNetApi:Uri"]
            ?? throw new ArgumentNullException("A configuração para 'EmpregaNetApi:Uri' não foi encontrada.");

        services.AddResilientRefitClient<IRestApiClient>(EmpregaNetApi);

        return services;
    }
}
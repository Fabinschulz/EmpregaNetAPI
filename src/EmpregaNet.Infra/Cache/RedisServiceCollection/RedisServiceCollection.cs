using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EmpregaNet.Infra.Cache;

public static class RedisServiceCollection
{
    public static IServiceCollection UseRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Redis"];

        if (!string.IsNullOrEmpty(connectionString))
        {

            Action<ConfigurationOptions> defaultOptions = (opts) =>
            {
                opts.AbortOnConnectFail = false;
                opts.ReconnectRetryPolicy = new ExponentialRetry(2000, 5000);
                opts.ConnectRetry = 3;
                opts.ConnectTimeout = 5000;
                opts.SyncTimeout = 5000;
                opts.AsyncTimeout = 5000;
                opts.KeepAlive = 30;
            };

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString, defaultOptions);
            services.AddSingleton<IConnectionMultiplexer>(redis);

            Console.WriteLine("Redis IsConnected: " + redis.IsConnected);
        }
        else
        {
            Console.WriteLine("A configuração do Redis não foi encontrada. Verifique a chave 'Redis' no arquivo de configuração.");
        }

        return services;
    }

    public static IServiceCollection UseMemoryService(this IServiceCollection services, Action<MemoryServiceOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddMemoryCache().AddDataProtection();
        services.AddSingleton<IMemoryService, MemoryService>();
        return services;
    }
}

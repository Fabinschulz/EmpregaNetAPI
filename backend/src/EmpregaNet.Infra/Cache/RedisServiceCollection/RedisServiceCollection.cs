using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EmpregaNet.Infra.Cache;

public static class RedisServiceCollection
{
    public static WebApplicationBuilder UseRedisCache(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration["Redis"];

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
            builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

            Console.WriteLine("Redis IsConnected: " + redis.IsConnected);
        }
        else
        {
            Console.WriteLine("A configuração do Redis não foi encontrada. Verifique a chave 'Redis' no arquivo de configuração.");
        }

        return builder;
    }

    public static WebApplicationBuilder UseMemoryService(this WebApplicationBuilder builder, Action<MemoryServiceOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.Services.AddMemoryCache().AddDataProtection();
        builder.Services.AddSingleton<IMemoryService, MemoryService>();
        
        return builder;
    }
}

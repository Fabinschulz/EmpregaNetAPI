using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EmpregaNet.Infra.Cache;

public static class RedisServiceCollection
{
    public static WebApplicationBuilder UseRedisCache(this WebApplicationBuilder builder)
    {
        var redisOptions = RedisOptions.Resolve(builder.Configuration);
        builder.Services.Configure<RedisOptions>(opts =>
        {
            opts.Enabled = redisOptions.Enabled;
            opts.ConnectionString = redisOptions.ConnectionString;
            opts.InstanceName = redisOptions.InstanceName;
        });

        if (!redisOptions.IsActive)
        {
            Console.WriteLine("Redis desabilitado; Output Cache usará memória local (in-memory).");
            return builder;
        }

        var redis = ConnectionMultiplexer.Connect(redisOptions.ConnectionString, ConfigureConnection);
        builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
        Console.WriteLine($"Redis conectado (Output Cache distribuído): {redis.IsConnected}");

        return builder;
    }

    private static void ConfigureConnection(ConfigurationOptions opts)
    {
        opts.AbortOnConnectFail = false;
        opts.ReconnectRetryPolicy = new ExponentialRetry(2000, 5000);
        opts.ConnectRetry = 3;
        opts.ConnectTimeout = 5000;
        opts.SyncTimeout = 5000;
        opts.AsyncTimeout = 5000;
        opts.KeepAlive = 30;
    }
}

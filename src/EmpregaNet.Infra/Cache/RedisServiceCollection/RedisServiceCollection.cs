using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EmpregaNet.Infra.Cache.ElastiCacheRedis
{
    public static class RedisServiceCollection
    {
        public static IServiceCollection UseRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            var redisEndpoint = configuration["Redis"];

            if (string.IsNullOrEmpty(redisEndpoint))
            {
                throw new ArgumentException("A configuração do Redis não foi encontrada. Verifique a chave 'Redis' no arquivo de configuração.");
            }

            Action<ConfigurationOptions> configDefault = (ConfigurationOptions opts) =>
            {
                opts.AbortOnConnectFail = false;
                opts.ReconnectRetryPolicy = new ExponentialRetry(2000, 5000);
                opts.ConnectRetry = 3;
                opts.ConnectTimeout = 5000;
                opts.SyncTimeout = 5000;
                opts.AsyncTimeout = 5000;
                opts.KeepAlive = 30;
            };

            services.AddStackExchangeRedisCache(options =>
             {
                 options.Configuration = redisEndpoint;
                 options.InstanceName = "EMPREGANET_";
             });

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisEndpoint, configDefault);
            services.AddSingleton<IConnectionMultiplexer>(redis);

            Console.WriteLine("Redis IsConnected: " + redis.IsConnected);

            return services;
        }
    }
}
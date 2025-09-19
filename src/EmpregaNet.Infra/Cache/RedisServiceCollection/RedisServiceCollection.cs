using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EmpregaNet.Infra.Cache.RedisServiceCollection
{
    public static class RedisServiceCollection
    {
        public static IServiceCollection UseRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["Redis"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("A configuração do Redis não foi encontrada. Verifique a chave 'Redis' no arquivo de configuração.");
            }

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

            return services;
        }
    }
}
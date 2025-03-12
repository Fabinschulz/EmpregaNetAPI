using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EmpregaNet.Infra.Cache.ElastiCacheRedis
{
    public static class ElastiCacheRedis
    {
        public static void RedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            var redisEndpoint = configuration["Redis"];

            if (string.IsNullOrEmpty(redisEndpoint))
            {
                throw new ArgumentException("Missing configuration values for ElastiCache Redis");
            }

            try
            {

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisEndpoint;
                    options.InstanceName = "EMPREGANET_";
                });

                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisEndpoint);
                services.AddSingleton<IConnectionMultiplexer>(redis);
                Console.WriteLine("Redis IsConnected: " + redis.IsConnected);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao conectar ao Redis: " + ex.Message);
                throw;
            }
        }
    }
}
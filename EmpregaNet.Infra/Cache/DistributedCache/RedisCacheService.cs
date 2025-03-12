using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace EmpregaNet.Infra.Cache.DistributedCache
{
    public class RedisCacheService : IDistributedCacheService
    {
        private readonly IDatabase _redisDb;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _redisDb = redis.GetDatabase() ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger;
        }

        public async Task<T?> GetValueAsync<T>(string cacheKey)
        {
            try
            {
                _logger.LogInformation("Buscando dados do cache para a chave: {CacheKey}", cacheKey);
                var cachedData = await _redisDb.StringGetAsync(cacheKey);

                if (!cachedData.IsNullOrEmpty)
                {
                    _logger.LogInformation("Dados encontrados no cache para a chave: {CacheKey}", cacheKey);
                    return JsonSerializer.Deserialize<T>(cachedData!);
                }

                _logger.LogInformation("Dados não encontrados no cache para a chave: {CacheKey}", cacheKey);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados do cache para a chave: {CacheKey}", cacheKey);
                throw;
            }
        }

        public async Task SetValueAsync<T>(string cacheKey, T data, DistributedCacheEntryOptions options)
        {
            try
            {
                _logger.LogInformation("Armazenando dados no cache para a chave: {CacheKey}", cacheKey);
                var jsonData = JsonSerializer.Serialize(data);
                var expiry = options.AbsoluteExpirationRelativeToNow;
                await _redisDb.StringSetAsync(cacheKey, jsonData, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao armazenar dados no cache para a chave: {CacheKey}", cacheKey);
                throw;
            }
        }

        public async Task<bool> InvalidateCacheAsync(string cacheKey)
        {
            _logger.LogInformation("Removendo dados do cache para a chave: {CacheKey}", cacheKey);
            return await _redisDb.KeyDeleteAsync(cacheKey);
        }

        public DistributedCacheEntryOptions GetCacheOptions()
        {
            return new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(5)
            };
        }
    }
}
using Microsoft.Extensions.Caching.Distributed;

namespace EmpregaNet.Infra.Cache.DistributedCache
{
    public interface IDistributedCacheService
    {
        Task<T?> GetValueAsync<T>(string cacheKey);
        Task SetValueAsync<T>(string cacheKey, T data, DistributedCacheEntryOptions options);
        Task<bool> InvalidateCacheAsync(string cacheKey);
        DistributedCacheEntryOptions GetCacheOptions();
    }
}
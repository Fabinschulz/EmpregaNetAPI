namespace EmpregaNet.Infra.Cache.MemoryService
{
    public interface IMemoryService
    {
        Task<T>? Get<T>(string cachekey, Func<T, bool> isValid);
        Task<T>? Get<T>(string cacheKey);
        Task SetValueAsync<T>(string cacheKey, T obj, TimeSpan expiration);
        void Remove(string cacheKey);
    }
}
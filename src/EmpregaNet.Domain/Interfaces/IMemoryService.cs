namespace EmpregaNet.Domain.Interfaces;

public interface IMemoryService
{
    Task<T?> GetValueAsync<T>(string cachekey, Func<T, bool> isValid);
    Task<T?> GetValueAsync<T>(string cacheKey);
    Task SetValueAsync<T>(string cacheKey, T obj, TimeSpan expiration);
    void Remove(string cacheKey);
    Task RemoveByPatternAsync(string pattern);
}

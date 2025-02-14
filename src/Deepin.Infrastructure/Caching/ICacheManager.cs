namespace Deepin.Infrastructure.Caching;
public interface ICacheManager
{
    Task<T?> GetAsync<T>(string key);
    Task<IEnumerable<T>?> GetAsync<T>(string[] keys);
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> acquire);
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> acquire, TimeSpan absoluteExpiration);
    Task<bool> IsSetAsync(string key);
    Task SetAsync<T>(string key, T data);
    Task SetAsync<T>(string key, T data, TimeSpan absoluteExpiration);
    Task RemoveAsync(string key);
    Task RemoveByPrefix(string prefix);
    Task ClearAsync();
}

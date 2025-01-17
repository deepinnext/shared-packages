using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Deepin.Infrastructure.Extensions;

public static class DistributedCacheExtensions
{
    public static async Task<T?> GetOrCreateAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> acquire, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
    {
        T? result = default;
        if (string.IsNullOrEmpty(key))
        {
            return result;
        }
        result = await cache.GetAsync<T>(key);

        if (result is null)
        {
            result = await acquire();
            if (result != null)
            {
                await cache.SetStringAsync(key, JsonConvert.SerializeObject(result), options, cancellationToken);
            }
        }
        return result;
    }

    public static async Task<T?> GetOrCreateAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> acquire, CancellationToken cancellationToken = default)
    => await cache.GetOrCreateAsync<T>(key, acquire, new DistributedCacheEntryOptions()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
    }, cancellationToken);

    public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key)
    {
        if (string.IsNullOrEmpty(key))
            return default;
        string? value = await cache.GetStringAsync(key);
        if (value is null)
            return default;
        var item = JsonConvert.DeserializeObject<T>(value);
        return item;
    }
}
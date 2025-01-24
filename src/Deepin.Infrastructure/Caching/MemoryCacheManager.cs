﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Deepin.Infrastructure.Caching;
internal class MemoryCacheManager(IMemoryCache memoryCache, CacheOptions options) : ICacheManager
{
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _keys = new ConcurrentDictionary<string, CancellationTokenSource>();
    private static CancellationTokenSource _cancellationToken = new();
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly CacheOptions _options = options;


    public async Task ClearAsync()
    {
        Parallel.ForEach(_keys, async item =>
        {
            await this.RemoveAsync(item.Key);
        });
        _cancellationToken.Cancel();
        _cancellationToken.Dispose();
        _cancellationToken = new CancellationTokenSource();
        await Task.FromResult(0);
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _memoryCache.TryGetValue(key, out T? result);
        return Task.FromResult(result);
    }

    public async Task<IEnumerable<T>?> GetAsync<T>(string[] keys)
    {
        if (keys == null || keys.Length == 0)
            return null;
        var result = new List<T>();
        foreach (var key in keys)
        {
            var item = await this.GetAsync<T>(key);
            if (item != null)
                result.Add(item);
        }
        return result;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> acquire)
    {
        return await GetOrSetAsync(key, acquire, _options.DefaultCacheTimeMinutes);
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> acquire, int cacheTime)
    {
        T? result = default;
        if (string.IsNullOrEmpty(key))

        {
            return result;
        }
        result = await this.GetAsync<T>(key);
        if (result == null)
        {
            result = await acquire();
            if (result != null)
            {
                await this.SetAsync(key, result, cacheTime);
            }
        }
        return result;
    }

    public async Task<bool> IsSetAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        return await Task.FromResult(_keys.Any(x => x.Key == key));
    }

    public async Task RemoveAsync(string key)
    {
        if (await IsSetAsync(key))
        {
            var removed = _keys.TryRemove(key, out var cancellationToken);
            if (removed)
                _memoryCache.Remove(key);
        }
    }

    public async Task RemoveByPrefix(string prefix)
    {
        var keys = _keys.Where(x => x.Key.StartsWith(prefix));
        if (keys.Any())
        {
            Parallel.ForEach(keys, async (item) =>
            {
                await this.RemoveAsync(item.Key);
            });
        }
        await Task.FromResult(0);
    }

    public async Task SetAsync<T>(string key, T data)
    {
        await this.SetAsync(key, data, _options.DefaultCacheTimeMinutes);
    }

    public async Task SetAsync<T>(string key, T data, int cacheTime)
    {
        var added = _keys.TryAdd(key, _cancellationToken);
        if (added)
            _memoryCache.Set(key, data, TimeSpan.FromMinutes(cacheTime));
        await Task.FromResult(0);
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Deepin.Infrastructure.Caching;
public class RedisCacheManager(IConnectionMultiplexer redis, RedisCacheOptions options) : ICacheManager
{
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly IDatabase _database = redis.GetDatabase();
    private readonly RedisCacheOptions _options = options;
    private IEnumerable<RedisKey> GetKeys(EndPoint endPoint, string? prefix = null)
    {
        var server = _redis.GetServer(endPoint);
        var keys = server.Keys(_database.Database, string.IsNullOrEmpty(prefix) ? null : $"{prefix}*");
        return keys;
    }
    public async Task ClearAsync()
    {
        foreach (var endPoint in _redis.GetEndPoints())
        {
            var keys = this.GetKeys(endPoint);
            if (keys != null && keys.Any())
                await _database.KeyDeleteAsync(keys.ToArray());
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
            return default;
        var serializedItem = await _database.StringGetAsync(key);
        if (!serializedItem.HasValue)
            return default;
        var item = JsonConvert.DeserializeObject<T>(serializedItem.ToString());
        return item;
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
        if (await IsSetAsync(key))
        {
            result = await this.GetAsync<T>(key);
        }
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
        return await _database.KeyExistsAsync(key);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task RemoveByPrefix(string prefix)
    {
        foreach (var endPoint in _redis.GetEndPoints())
        {
            var keys = this.GetKeys(endPoint, prefix);
            if (keys != null && keys.Any())
                await _database.KeyDeleteAsync(keys.ToArray());
        }
    }

    public Task SetAsync<T>(string key, T data)
    {
        return this.SetAsync(key, data, _options.DefaultCacheTimeMinutes);
    }

    public async Task SetAsync<T>(string key, T data, int cacheTimeMinutes)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (cacheTimeMinutes <= 0)
            return;

        if (data == null)
            return;

        var serializedItem = JsonConvert.SerializeObject(data);
        await _database.StringSetAsync(key, serializedItem, TimeSpan.FromMinutes(cacheTimeMinutes));
    }
}

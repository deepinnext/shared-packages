namespace Deepin.Infrastructure.Caching;

public class RedisCacheOptions : CacheOptions
{
    public required string ConnectionString { get; set; }
}

using Microsoft.Extensions.Caching.Memory;

namespace Tavstal.MesterMC.Api.Models;

public class ExpiringCache<TKey, TValue>
{
    private readonly IMemoryCache cache;
    private readonly TimeSpan expiration;

    public ExpiringCache(TimeSpan expiration)
    {
        this.cache = new MemoryCache(new MemoryCacheOptions());
        this.expiration = expiration;
    }

    public void Set(TKey key, TValue value)
    {
        cache.Set(key, value, expiration);
    }

    public bool TryGet(TKey key, out TValue value)
    {
        return cache.TryGetValue(key, out value);
    }

    public void Remove(TKey key)
    {
        cache.Remove(key);
    }
}
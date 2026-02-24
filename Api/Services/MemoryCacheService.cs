using Microsoft.Extensions.Caching.Memory;

namespace Tavstal.MesterMC.Api.Services;

public class MemoryCacheService
{
    private readonly IMemoryCache _cache;
    
    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }
    
    public bool TryGetValue<T>(string key, out T? value)
    {
        return _cache.TryGetValue(key, out value);
    }
    
    public void SetValue<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (absoluteExpirationRelativeToNow.HasValue)
            options.SetAbsoluteExpiration(absoluteExpirationRelativeToNow.Value);
        
        _cache.Set(key, value, options);
    }
    
    public void RemoveValue(string key)
    {
        _cache.Remove(key);
    }
}
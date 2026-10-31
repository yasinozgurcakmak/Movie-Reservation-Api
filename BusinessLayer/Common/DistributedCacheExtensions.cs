using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Common;

public static class DistributedCacheExtensions
{
    public static async Task<T> GetOrCreateAsync<T>(
        this IDistributedCache cache, string key, TimeSpan duration, Func<Task<T>> factory, ILogger logger)
    {
        try
        {
            var cached = await cache.GetStringAsync(key);
            if (cached is not null) return JsonSerializer.Deserialize<T>(cached)!;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache read failed for {CacheKey}", key);
        }

        var value = await factory();

        try
        {
            await cache.SetStringAsync(key, JsonSerializer.Serialize(value),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = duration });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache write failed for {CacheKey}", key);
        }

        return value;
    }

    public static async Task TryRemoveAsync(this IDistributedCache cache, string key, ILogger logger)
    {
        try
        {
            await cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache remove failed for {CacheKey}", key);
        }
    }
}

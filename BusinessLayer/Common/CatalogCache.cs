using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Common;

public class CatalogCache
{
    private const string VersionKey = "catalog:version";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly IDistributedCache _cache;
    private readonly ILogger<CatalogCache> _logger;

    public CatalogCache(IDistributedCache cache, ILogger<CatalogCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
    {
        string? version;
        try
        {
            version = await _cache.GetStringAsync(VersionKey);
            if (version is null)
            {
                version = Guid.NewGuid().ToString("N");
                await _cache.SetStringAsync(VersionKey, version);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for {CacheKey}", VersionKey);
            return await factory();
        }

        return await _cache.GetOrCreateAsync($"catalog:{version}:{key}", CacheDuration, factory, _logger);
    }

    public Task InvalidateAsync() => _cache.TryRemoveAsync(VersionKey, _logger);
}

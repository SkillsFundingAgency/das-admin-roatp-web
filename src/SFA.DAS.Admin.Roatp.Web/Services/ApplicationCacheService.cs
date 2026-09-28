using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class ApplicationCacheService(IDistributedCache distributedCache) : IApplicationCacheService
{
    public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromHours(4);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var json = await distributedCache.GetStringAsync(key, cancellationToken);
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? cacheDuration = null,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheDuration ?? DefaultCacheDuration
        };

        await distributedCache.SetStringAsync(key, json, options, cancellationToken);
    }
}

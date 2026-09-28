using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class ApplicationCacheService(IDistributedCache distributedCache) : IApplicationCacheService
{
    public static readonly TimeSpan DefaultExpiry = TimeSpan.FromHours(4);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var json = await distributedCache.GetStringAsync(key, cancellationToken);
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry
        };

        await distributedCache.SetStringAsync(key, json, options, cancellationToken);
    }
}

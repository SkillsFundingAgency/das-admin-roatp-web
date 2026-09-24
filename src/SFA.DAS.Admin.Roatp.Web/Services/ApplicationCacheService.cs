using Microsoft.Extensions.Caching.Memory;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class ApplicationCacheService(IMemoryCache memoryCache) : IApplicationCacheService
{
    public const int DefaultExpirationMinutes = 30;

    public bool TryGet<T>(string key, out T? value)
    {
        if (memoryCache.TryGetValue(key, out T? cached) && cached is not null)
        {
            value = cached;
            return true;
        }

        value = default;
        return false;
    }

    public void Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)
    {
        memoryCache.Set(
            key,
            value,
            absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(DefaultExpirationMinutes));
    }
}

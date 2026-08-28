namespace SFA.DAS.Admin.Roatp.Web.Services;

public class ScopedCacheService(IHttpContextAccessor httpContextAccessor) : IScopedCacheService
{
    private const string CacheItemsKey = "ScopedCacheService.Cache";

    public bool TryGetValue<TKey, TValue>(TKey key, out TValue? value)
        where TKey : notnull
    {
        if (GetCache().TryGetValue(key, out var cached))
        {
            value = (TValue?)cached;
            return true;
        }

        value = default;
        return false;
    }

    public void Set<TKey, TValue>(TKey key, TValue value)
        where TKey : notnull
    {
        GetCache()[key] = value;
    }

    private Dictionary<object, object?> GetCache()
    {
        var items = httpContextAccessor.HttpContext?.Items
            ?? throw new InvalidOperationException("HttpContext is not available.");

        if (items[CacheItemsKey] is Dictionary<object, object?> cache)
        {
            return cache;
        }

        cache = [];
        items[CacheItemsKey] = cache;
        return cache;
    }
}

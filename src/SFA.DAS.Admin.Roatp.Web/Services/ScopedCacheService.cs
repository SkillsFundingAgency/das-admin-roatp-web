namespace SFA.DAS.Admin.Roatp.Web.Services;

public class ScopedCacheService(IHttpContextAccessor httpContextAccessor) : IScopedCacheService
{
    public bool TryGetValue<TKey, TValue>(string cacheKey, TKey key, out TValue? value)
        where TKey : notnull
    {
        if (GetCache<TKey>(cacheKey).TryGetValue(key, out var cached))
        {
            value = (TValue?)cached;
            return true;
        }

        value = default;
        return false;
    }

    public void Set<TKey, TValue>(string cacheKey, TKey key, TValue value)
        where TKey : notnull
    {
        GetCache<TKey>(cacheKey)[key] = value;
    }

    private Dictionary<TKey, object?> GetCache<TKey>(string cacheKey)
        where TKey : notnull
    {
        var items = httpContextAccessor.HttpContext?.Items
            ?? throw new InvalidOperationException("HttpContext is not available.");

        if (items[cacheKey] is Dictionary<TKey, object?> cache)
        {
            return cache;
        }

        cache = new Dictionary<TKey, object?>();
        items[cacheKey] = cache;
        return cache;
    }
}

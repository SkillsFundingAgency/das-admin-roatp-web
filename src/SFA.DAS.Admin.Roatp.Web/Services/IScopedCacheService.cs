namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IScopedCacheService
{
    bool TryGetValue<TKey, TValue>(string cacheKey, TKey key, out TValue? value)
        where TKey : notnull;

    void Set<TKey, TValue>(string cacheKey, TKey key, TValue value)
        where TKey : notnull;
}

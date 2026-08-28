namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IScopedCacheService
{
    bool TryGetValue<TKey, TValue>(TKey key, out TValue? value)
        where TKey : notnull;

    void Set<TKey, TValue>(TKey key, TValue value)
        where TKey : notnull;
}

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IApplicationCacheService
{
    bool TryGet<T>(string key, out T? value);
    void Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null);
}

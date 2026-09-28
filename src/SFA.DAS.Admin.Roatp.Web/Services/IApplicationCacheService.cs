namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IApplicationCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? cacheDuration = null, CancellationToken cancellationToken = default);
}

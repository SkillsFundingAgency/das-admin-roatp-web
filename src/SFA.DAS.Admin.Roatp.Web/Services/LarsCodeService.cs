using System.Net;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class LarsCodeService(IOuterApiClient outerApiClient, IHttpContextAccessor httpContextAccessor) : ILarsCodeService
{
    private const string CacheItemsKey = "LarsCodeService.Cache";

    public async Task<GetRestrictedCourseDetailsResponse?> GetCourseDetailsAsync(string larsCode, CancellationToken cancellationToken)
    {
        var cache = GetCache();

        if (cache.TryGetValue(larsCode, out var cached))
        {
            return cached;
        }

        var response = await outerApiClient.GetAllowedProvidersForCourse(larsCode, cancellationToken);

        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
        {
            throw new HttpRequestException(
                $"Failed to retrieve course details for LARS code '{larsCode}'. Status code: {response.StatusCode}.");
        }

        var courseDetails = response.StatusCode == HttpStatusCode.NotFound ? null : response.Content;
        cache[larsCode] = courseDetails;

        return courseDetails;
    }

    private Dictionary<string, GetRestrictedCourseDetailsResponse?> GetCache()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        if (httpContext.Items[CacheItemsKey] is not Dictionary<string, GetRestrictedCourseDetailsResponse?> cache)
        {
            cache = new Dictionary<string, GetRestrictedCourseDetailsResponse?>(StringComparer.OrdinalIgnoreCase);
            httpContext.Items[CacheItemsKey] = cache;
        }

        return cache;
    }
}

using System.Net;
using Refit;
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
        var courseDetails = ExtractCourseDetails(larsCode, response);

        cache[larsCode] = courseDetails;

        return courseDetails;
    }

    private static GetRestrictedCourseDetailsResponse? ExtractCourseDetails(
        string larsCode,
        ApiResponse<GetRestrictedCourseDetailsResponse> response)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to retrieve course details for LARS code '{larsCode}'. Status code: {response.StatusCode}.");
        }

        return response.Content;
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

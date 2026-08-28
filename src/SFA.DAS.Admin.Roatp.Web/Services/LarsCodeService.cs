using System.Net;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class LarsCodeService(IOuterApiClient outerApiClient, IScopedCacheService scopedCacheService) : ILarsCodeService
{
    public async Task<GetRestrictedCourseDetailsResponse?> GetCourseDetailsAsync(
        string larsCode,
        CancellationToken cancellationToken)
    {
        if (scopedCacheService.TryGetValue(larsCode, out GetRestrictedCourseDetailsResponse? cached))
        {
            return cached;
        }

        var response = await outerApiClient.GetAllowedProvidersForCourse(larsCode, cancellationToken);
        var courseDetails = ExtractCourseDetails(larsCode, response);
        scopedCacheService.Set(larsCode, courseDetails);
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
}

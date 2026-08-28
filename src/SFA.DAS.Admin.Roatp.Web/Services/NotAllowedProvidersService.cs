using System.Net;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class NotAllowedProvidersService(IOuterApiClient outerApiClient, ISessionService sessionService)
    : INotAllowedProvidersService
{
    public async Task<GetRestrictedCourseDetailsResponse?> GetNotAllowedProvidersAsync(
        string larsCode,
        CancellationToken cancellationToken)
    {
        var cacheKey = SessionKeys.NotAllowedProvidersForRestrictedCourse(larsCode);

        if (sessionService.Contains(cacheKey))
        {
            return sessionService.Get<GetRestrictedCourseDetailsResponse>(cacheKey);
        }

        var response = await outerApiClient.GetNotAllowedProvidersForCourse(larsCode, cancellationToken);
        var courseDetails = ExtractCourseDetails(larsCode, response);

        sessionService.Set(cacheKey, courseDetails);

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
                $"Failed to retrieve not-allowed providers for LARS code '{larsCode}'. Status code: {response.StatusCode}.");
        }

        return response.Content;
    }
}

using System.Net;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class OuterApiClientExtensions
{
    public static async Task<GetRestrictedCourseDetailsResponse?> GetCourseDetails(
        this IOuterApiClient outerApiClient,
        string larsCode,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetAllowedProvidersForCourse(larsCode, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync();
        return response.Content;
    }

    public static async Task<ProviderAllowedCourseModel?> GetAllowedCourse(
        this IOuterApiClient outerApiClient,
        int ukprn,
        string larsCode,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetProviderAllowedCourse(ukprn, larsCode, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync();
        return response.Content;
    }
}

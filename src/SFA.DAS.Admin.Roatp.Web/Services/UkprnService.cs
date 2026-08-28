using System.Net;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class UkprnService(IOuterApiClient outerApiClient, IScopedCacheService scopedCacheService) : IUkprnService
{
    public async Task<GetOrganisationResponse?> GetOrganisationAsync(int ukprn, CancellationToken cancellationToken)
    {
        if (scopedCacheService.TryGetValue(ukprn, out GetOrganisationResponse? cached))
        {
            return cached;
        }

        var response = await outerApiClient.GetOrganisation(ukprn, cancellationToken);
        var organisation = ExtractOrganisation(ukprn, response);
        scopedCacheService.Set(ukprn, organisation);
        return organisation;
    }

    private static GetOrganisationResponse? ExtractOrganisation(
        int ukprn,
        ApiResponse<GetOrganisationResponse> response)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to retrieve organisation for UKPRN '{ukprn}'. Status code: {response.StatusCode}.");
        }

        return response.Content;
    }
}

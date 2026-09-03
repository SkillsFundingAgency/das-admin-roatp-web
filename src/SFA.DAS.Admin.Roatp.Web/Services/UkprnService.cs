using System.Net;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
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
        var organisation = await ExtractOrganisationAsync(response);
        scopedCacheService.Set(ukprn, organisation);
        return organisation;
    }

    private static async Task<GetOrganisationResponse?> ExtractOrganisationAsync(
        ApiResponse<GetOrganisationResponse> response)
    {
        if (response.IsNotFound())
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync();
        return response.Content;
    }
}

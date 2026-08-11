using System.Net;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class UkprnService(IOuterApiClient outerApiClient, IHttpContextAccessor httpContextAccessor) : IUkprnService
{
    private const string CacheItemsKey = "UkprnService.Cache";

    public async Task<GetOrganisationResponse?> GetOrganisationAsync(int ukprn, CancellationToken cancellationToken)
    {
        var cache = GetCache();

        if (cache.TryGetValue(ukprn, out var cached))
        {
            return cached;
        }

        var response = await outerApiClient.GetOrganisation(ukprn, cancellationToken);
        var organisation = ExtractOrganisation(ukprn, response);

        cache[ukprn] = organisation;

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

    private Dictionary<int, GetOrganisationResponse?> GetCache()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        if (httpContext.Items[CacheItemsKey] is not Dictionary<int, GetOrganisationResponse?> cache)
        {
            cache = new Dictionary<int, GetOrganisationResponse?>();
            httpContext.Items[CacheItemsKey] = cache;
        }

        return cache;
    }
}

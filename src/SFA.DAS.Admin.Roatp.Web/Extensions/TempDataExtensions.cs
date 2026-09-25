using System.Net;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class TempDataExtensions
{
    public static async Task<string?> GetProviderName(
        this ITempDataDictionary tempData,
        IOuterApiClient outerApiClient,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var cachedProviderName = tempData.Peek(TempDataKeys.ProviderLegalName) as string;
        if (!string.IsNullOrWhiteSpace(cachedProviderName))
        {
            tempData.Keep(TempDataKeys.ProviderLegalName);
            return cachedProviderName;
        }

        var organisationApiResponse = await outerApiClient.GetOrganisation(ukprn, cancellationToken);
        if (organisationApiResponse.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var providerName = organisationApiResponse.Content!.LegalName;
        tempData[TempDataKeys.ProviderLegalName] = providerName;
        return providerName;
    }
}

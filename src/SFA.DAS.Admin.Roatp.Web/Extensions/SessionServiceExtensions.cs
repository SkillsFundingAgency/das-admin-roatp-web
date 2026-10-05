using System.Net;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class SessionServiceExtensions
{
    private static string? GetProviderNameFromSession(this ISessionService sessionService, int ukprn)
    {
        var providerNameFromSession = sessionService.Get<Dictionary<int, string>>(SessionKeys.ProviderNames);
        if (providerNameFromSession is not null
            && providerNameFromSession.TryGetValue(ukprn, out var providerName)
            && !string.IsNullOrWhiteSpace(providerName))
        {
            return providerName;
        }

        return null;
    }

    public static void SetProviderName(this ISessionService sessionService, int ukprn, string providerName)
    {
        var providerNameSession = sessionService.Get<Dictionary<int, string>>(SessionKeys.ProviderNames) ?? [];
        providerNameSession[ukprn] = providerName;
        sessionService.Set(SessionKeys.ProviderNames, providerNameSession);
    }

    public static async Task<string?> GetProviderName(
        this ISessionService sessionService,
        IOuterApiClient outerApiClient,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var providerNameSession = sessionService.GetProviderNameFromSession(ukprn);
        if (providerNameSession is not null)
        {
            return providerNameSession;
        }

        var response = await outerApiClient.GetOrganisation(ukprn, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync();
        var providerName = response.Content?.LegalName;
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return null;
        }

        sessionService.SetProviderName(ukprn, providerName);
        return providerName;
    }
}

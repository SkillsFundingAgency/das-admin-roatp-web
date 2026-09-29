using System.Net;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class SessionServiceExtensions
{
    public static string? GetProviderNameSession(this ISessionService sessionService, int ukprn)
    {
        var providerNameSession = sessionService.Get<Dictionary<int, string>>(SessionKeys.ProviderName);
        if (providerNameSession is not null
            && providerNameSession.TryGetValue(ukprn, out var providerName)
            && !string.IsNullOrWhiteSpace(providerName))
        {
            return providerName;
        }

        return null;
    }

    public static void SetProviderName(this ISessionService sessionService, int ukprn, string providerName)
    {
        var providerNameSession = sessionService.Get<Dictionary<int, string>>(SessionKeys.ProviderName) ?? [];
        providerNameSession[ukprn] = providerName;
        sessionService.Set(SessionKeys.ProviderName, providerNameSession);
    }

    public static async Task<string?> GetProviderName(
        this ISessionService sessionService,
        IOuterApiClient outerApiClient,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var providerNameSession = sessionService.GetProviderNameSession(ukprn);
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

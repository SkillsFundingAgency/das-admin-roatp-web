using System.Net;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class SessionServiceExtensions
{
    public static string? GetProviderNameSession(this ISessionService sessionService, int ukprn)
    {
        var providerNameSession = sessionService.Get<ProviderNameSessionModel>(SessionKeys.ProviderName(ukprn));
        if (providerNameSession is not null
            && providerNameSession.Ukprn == ukprn
            && !string.IsNullOrWhiteSpace(providerNameSession.ProviderName))
        {
            return providerNameSession.ProviderName;
        }

        return null;
    }

    public static void SetProviderName(this ISessionService sessionService, int ukprn, string providerName)
    {
        sessionService.Set(SessionKeys.ProviderName(ukprn), new ProviderNameSessionModel
        {
            Ukprn = ukprn,
            ProviderName = providerName
        });
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

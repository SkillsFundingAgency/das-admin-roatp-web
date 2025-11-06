using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;


[Route("providers/{ukprn}/status-confirmed", Name = RouteNames.ProviderStatusUpdateConfirmed)]
public class ProviderStatusConfirmationController(IOuterApiClient _outerApiClient) : Controller
{
    [Authorize(Roles = Roles.RoatpAdminTeam)]

    public async Task<IActionResult> Index(string ukprn, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationResponse == null || organisationResponse.Ukprn.ToString() != ukprn) return RedirectToRoute(RouteNames.Home);

        var model = new ProviderStatusConfirmationViewModel
        {
            LegalName = organisationResponse.LegalName,
            OrganisationStatus = organisationResponse.Status,
            Ukprn = ukprn,
            StatusText = MatchingStatusText(organisationResponse.Status),
            ProviderSummaryLink = Url.RouteUrl(RouteNames.ProviderSummary, new { ukprn })!,
            SelectTrainingProviderLink = Url.RouteUrl(RouteNames.SelectProvider)!
        };

        return View(model);
    }

    private static string MatchingStatusText(OrganisationStatus status)
    {
        return status switch
        {
            OrganisationStatus.Active => "active",
            OrganisationStatus.ActiveNoStarts => "active but not taking on apprentices",
            OrganisationStatus.OnBoarding => "on-boarding",
            OrganisationStatus.Removed => "removed",
            _ => ""
        };
    }
}

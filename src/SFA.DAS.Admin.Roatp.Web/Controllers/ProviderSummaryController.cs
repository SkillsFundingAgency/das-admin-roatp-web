using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}", Name = RouteNames.ProviderSummary)]
public class ProviderSummaryController(IOuterApiClient _outerApiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string ukprn, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        ProviderSummaryViewModel model = organisationResponse;
        model.SearchProviderUrl = Url.RouteUrl(RouteNames.SelectProvider)!;
        model.StatusChangeLink = Url.RouteUrl(RouteNames.ProviderStatusUpdate, new { ukprn })!;
        model.ProviderTypeChangeLink = Url.RouteUrl(RouteNames.ProviderTypeUpdate, new { ukprn })!;
        model.OrganisationTypeChangeLink = Url.RouteUrl(RouteNames.OrganisationTypeUpdate, new { ukprn })!;
        return View(model);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using System.Net;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}", Name = RouteNames.ProviderSummary)]
public class ProviderSummaryController(IOuterApiClient _outerApiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var organisationApiResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationApiResponse.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        GetOrganisationResponse organisationResponse = organisationApiResponse.Content!;

        ProviderSummaryViewModel model = organisationResponse;
        model.SearchProviderUrl = Url.RouteUrl(RouteNames.SelectProvider)!;
        model.StatusChangeLink = Url.RouteUrl(RouteNames.ProviderStatusUpdate, new { ukprn })!;
        model.ProviderTypeChangeLink = Url.RouteUrl(RouteNames.ProviderTypeUpdate, new { ukprn })!;
        model.OrganisationTypeChangeLink = Url.RouteUrl(RouteNames.OrganisationTypeUpdate, new { ukprn })!;
        model.OffersApprenticeshipUnitsChangeLink = Url.RouteUrl(RouteNames.ApprenticeshipUnitsUpdate, new { ukprn })!;
        return View(model);
    }
}
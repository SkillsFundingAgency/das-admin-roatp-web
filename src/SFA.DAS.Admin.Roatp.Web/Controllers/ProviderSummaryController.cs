using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}", Name = RouteNames.ProviderSummary)]
public class ProviderSummaryController(IOuterApiClient _outerApiClient, ISessionService _sessionService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string ukprn, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var organisationSessionModel = (EditOrganisationSessionModel)organisationResponse;
        _sessionService.Set<EditOrganisationSessionModel>(SessionKeys.EditOrganisation, organisationSessionModel);

        ProviderSummaryViewModel model = organisationSessionModel;
        model.SearchProviderUrl = Url.RouteUrl(RouteNames.SelectProvider)!;
        return View(model);
    }
}
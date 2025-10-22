using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("ProviderSummary", Name = RouteNames.ProviderSummary)]
public class ProviderSummaryController(ISessionService _sessionService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var savedOrganisation = _sessionService.Get<EditOrganisationSessionModel>(SessionKeys.EditOrganisation);

        if (savedOrganisation == null) return RedirectToRoute(RouteNames.Home);

        ProviderSummaryViewModel model = (ProviderSummaryViewModel)savedOrganisation;
        model.SearchProviderUrl = Url.RouteUrl(RouteNames.SelectProvider)!;
        return View(model);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("providers/{ukprn}/status", Name = RouteNames.ProviderStatusUpdate)]
public class ProviderStatusUpdateController(IOuterApiClient _outerApiClient, IHttpContextAccessor _contextAccessor) : Controller
{
    [Authorize(Roles = Roles.RoatpAdminTeam)]

    public async Task<IActionResult> Index(string ukprn, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var model = new OrganisationStatusUpdateViewModel
        {
            OrganisationStatus = organisationResponse.Status,
            Ukprn = ukprn
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(int ukprn, OrganisationStatusUpdateViewModel model, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn.ToString(), cancellationToken);

        if (organisationResponse == null || organisationResponse.Ukprn != ukprn) return RedirectToRoute(RouteNames.Home);

        if (organisationResponse.Status == model.OrganisationStatus) return RedirectToRoute(RouteNames.ProviderSummary, new { model.Ukprn });

        string userId = _contextAccessor.HttpContext!.User.UserDisplayName();

        var patchDoc = new JsonPatchDocument<PatchOrganisationModel>();
        patchDoc.Replace(o => o.Status, model.OrganisationStatus);
        if (model.OrganisationStatus == OrganisationStatus.Removed)
        {
            // will redirect to  removedReason selection page in CSP-2212 
            var removedReasonIdOther = 12;
            patchDoc.Replace(o => o.RemovedReasonId, removedReasonIdOther);
        }

        await _outerApiClient.PatchOrganisation(ukprn.ToString(), userId, patchDoc, cancellationToken);

        return RedirectToRoute(RouteNames.ProviderStatusUpdateConfirmed, new { model.Ukprn });
    }
}

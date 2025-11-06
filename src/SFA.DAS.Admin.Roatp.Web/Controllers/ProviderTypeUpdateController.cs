using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("providers/{ukprn}/route", Name = RouteNames.ProviderTypeUpdate)]
public class ProviderTypeUpdateController(IOuterApiClient _outerApiClient, IOrganisationPatchService _organisationPatchService) : Controller
{
    [Authorize(Roles = Roles.RoatpAdminTeam)]
    public async Task<IActionResult> Index(string ukprn, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var model = new ProviderTypeUpdateViewModel
        {
            ProviderTypeId = (int)organisationResponse.ProviderType,
            ProviderTypes = BuildProviderTypes((int)organisationResponse.ProviderType)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(int ukprn, ProviderTypeUpdateViewModel model,
        CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn.ToString(), cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        PatchOrganisationModel patchModel = organisationResponse;
        patchModel.ProviderType = (ProviderType)model.ProviderTypeId;

        await _organisationPatchService.OrganisationPatched(ukprn, patchModel, cancellationToken);

        return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
    }

    private static List<OrganisationRouteSelectionModel> BuildProviderTypes(int providerTypeId)
    {
        return new List<OrganisationRouteSelectionModel>
        {
            new() { Description = "Main provider", Id = (int)ProviderType.Main, IsSelected = providerTypeId == (int)ProviderType.Main },
            new() { Description = "Employer provider", Id = (int)ProviderType.Employer, IsSelected = providerTypeId == (int)ProviderType.Employer },
            new() { Description = "Supporting provider", Id = (int)ProviderType.Supporting, IsSelected = providerTypeId == (int)ProviderType.Supporting },
        };
    }
}
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using System.Net;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("providers/{ukprn}/type", Name = RouteNames.OrganisationTypeUpdate)]

public class OrganisationTypeUpdateController(IOuterApiClient _outerApiClient, IOrganisationPatchService _organisationPatchService) : Controller
{
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var organisationApiResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationApiResponse.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        GetOrganisationResponse organisationResponse = organisationApiResponse.Content!;

        var organisationTypesResponse = await _outerApiClient.GetOrganisationTypes(cancellationToken);

        List<OrganisationTypeModel> organisationTypes = new List<OrganisationTypeModel>();

        foreach (var organisationType in organisationTypesResponse.OrganisationTypes)
        {
            organisationTypes.Add(new OrganisationTypeModel { Id = organisationType.Id, Description = organisationType.Description, IsSelected = organisationType.Id == organisationResponse.OrganisationTypeId });
        }

        var model = new OrganisationTypeUpdateViewModel
        {
            LegalName = organisationResponse.LegalName,
            OrganisationTypes = organisationTypes.OrderBy(r => r.Id).ToList(),
            OrganisationTypeId = organisationResponse.OrganisationTypeId
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(int ukprn, OrganisationTypeUpdateViewModel model,
        CancellationToken cancellationToken)
    {
        var organisationApiResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationApiResponse.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        GetOrganisationResponse organisationResponse = organisationApiResponse.Content!;

        PatchOrganisationModel patchModel = organisationResponse;
        patchModel.OrganisationTypeId = model.OrganisationTypeId;

        await _organisationPatchService.OrganisationPatched(ukprn, organisationResponse, patchModel, cancellationToken);

        return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
    }
}

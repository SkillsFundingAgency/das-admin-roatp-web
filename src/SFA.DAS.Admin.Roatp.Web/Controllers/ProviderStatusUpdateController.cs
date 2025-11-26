using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using System.Net;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("providers/{ukprn}/status", Name = RouteNames.ProviderStatusUpdate)]
public class ProviderStatusUpdateController(IOuterApiClient _outerApiClient, IOrganisationPatchService _organisationPatchService) : Controller
{
    private readonly string _providerStatusConfirmed = "~/Views/ProviderStatusUpdate/ProviderStatusConfirmation.cshtml";

    [Authorize(Roles = Roles.RoatpAdminTeam)]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var organisationApiResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationApiResponse.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        GetOrganisationResponse organisationResponse = organisationApiResponse.Content!;

        var model = new OrganisationStatusUpdateViewModel
        {
            OrganisationStatus = organisationResponse.Status,
            OrganisationStatuses = BuildOrganisationStatuses(organisationResponse.Status)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(int ukprn, OrganisationStatusUpdateViewModel model,
        CancellationToken cancellationToken)
    {
        var organisationApiResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationApiResponse.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        GetOrganisationResponse organisationResponse = organisationApiResponse.Content!;

        if (model.OrganisationStatus == OrganisationStatus.Removed)
        {
            return RedirectToRoute(RouteNames.ProviderRemovalReasonUpdate, new { ukprn });
        }

        PatchOrganisationModel patchModel = organisationResponse;
        patchModel.Status = model.OrganisationStatus;

        var organisationPatched = await _organisationPatchService.OrganisationPatched(ukprn, organisationResponse, patchModel, cancellationToken);

        return RedirectToRoute(!organisationPatched
            ? RouteNames.ProviderSummary
            : RouteNames.ProviderStatusUpdateConfirmed,
            new { ukprn });
    }

    [Authorize(Roles = Roles.RoatpAdminTeam)]
    [Route("confirmed", Name = RouteNames.ProviderStatusUpdateConfirmed)]
    public async Task<IActionResult> ProviderStatusUpdateConfirmed(int ukprn, CancellationToken cancellationToken)
    {
        var organisationApiResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationApiResponse.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        GetOrganisationResponse organisationResponse = organisationApiResponse.Content!;

        var model = new ProviderStatusConfirmationViewModel
        {
            LegalName = organisationResponse.LegalName,
            OrganisationStatus = organisationResponse.Status,
            Ukprn = ukprn,
            StatusText = MatchingStatusText(organisationResponse.Status),
            ProviderSummaryLink = Url.RouteUrl(RouteNames.ProviderSummary, new { ukprn })!,
            SelectTrainingProviderLink = Url.RouteUrl(RouteNames.SelectProvider)!,
            AddNewTrainingProviderLink = Url.RouteUrl(RouteNames.AddProvider)!
        };

        return View(_providerStatusConfirmed, model);
    }

    private static List<OrganisationStatusSelectionModel> BuildOrganisationStatuses(OrganisationStatus status)
    {
        return new List<OrganisationStatusSelectionModel>
        {
            new() { Description = "Active", Id = (int)OrganisationStatus.Active, IsSelected = status == OrganisationStatus.Active },
            new() { Description = "Active but not taking on apprentices", Id = (int)OrganisationStatus.ActiveNoStarts, IsSelected = status == OrganisationStatus.ActiveNoStarts },
            new() { Description = "On-boarding", Id = (int)OrganisationStatus.OnBoarding, IsSelected = status == OrganisationStatus.OnBoarding },
            new() { Description = "Removed", Id = (int)OrganisationStatus.Removed, IsSelected = status == OrganisationStatus.Removed }
        };
    }

    private static string MatchingStatusText(OrganisationStatus status)
    {
        return status switch
        {
            OrganisationStatus.ActiveNoStarts => "active but not taking on apprentices",
            OrganisationStatus.OnBoarding => "on-boarding",
            OrganisationStatus.Removed => "removed",
            _ => "active"
        };
    }
}

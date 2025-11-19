using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("providers/{ukprn}/removal-reason", Name = RouteNames.ProviderRemovalReasonUpdate)]
[Authorize(Roles = Roles.RoatpAdminTeam)]
public class RemovalReasonUpdateController(IOuterApiClient _outerApiClient, IOrganisationPatchService _organisationPatchService, IValidator<RemovalReasonUpdateViewModel> _validator) : Controller
{
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var removalReasonsResponse = await _outerApiClient.GetRemovalReasons(cancellationToken);

        List<RemovalReasonModel> removalReasons = new List<RemovalReasonModel>();

        foreach (var removalReason in removalReasonsResponse.ReasonsForRemoval)
        {
            removalReasons.Add(new RemovalReasonModel { Id = removalReason.Id, Description = removalReason.Description, IsSelected = removalReason.Id == organisationResponse.Content!.RemovedReasonId });
        }

        var model = new RemovalReasonUpdateViewModel
        {
            RemovedReasons = removalReasons.OrderBy(r => r.Description).ToList(),
            RemovalReasonId = organisationResponse.Content!.RemovedReasonId
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(int ukprn, RemovalReasonUpdateViewModel model, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var result = _validator.Validate(model);

        if (!result.IsValid)
        {
            var removalReasonsResponse = await _outerApiClient.GetRemovalReasons(cancellationToken);

            var viewModel = new RemovalReasonUpdateViewModel
            {
                RemovedReasons = removalReasonsResponse.ReasonsForRemoval.OrderBy(r => r.Description).ToList(),
                RemovalReasonId = organisationResponse.Content!.RemovedReasonId,
            };
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View(viewModel);
        }

        PatchOrganisationModel patchModel = organisationResponse.Content!;
        patchModel.Status = OrganisationStatus.Removed;
        patchModel.RemovedReasonId = model.RemovalReasonId;

        var organisationPatched = await _organisationPatchService.OrganisationPatched(ukprn, organisationResponse.Content!, patchModel, cancellationToken);

        return RedirectToRoute(!organisationPatched
            ? RouteNames.ProviderSummary
            : RouteNames.ProviderStatusUpdateConfirmed,
            new { ukprn });
    }
}
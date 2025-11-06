using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/apprenticeship-units", Name = RouteNames.ApprenticeshipUnitsUpdate)]
public class ApprenticeshipUnitsUpdateController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IOrganisationPatchService _organisationPatchService, IValidator<ApprenticeshipUnitsUpdateViewModel> _validator) : Controller
{
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var sessionModel = _sessionService.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes);
        var isProviderTypeAndCourseTypesChangeInProgress = sessionModel != null;

        var model = isProviderTypeAndCourseTypesChangeInProgress
            ? BuildViewModelForProviderTypeAndCourseTypesJourney(sessionModel!.CourseTypeIds)
            : BuildViewModelFromCurrentCourseTypes(organisationResponse.AllowedCourseTypes.ToList());

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(int ukprn, ApprenticeshipUnitsUpdateViewModel model,
        CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);
        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var sessionModel = _sessionService.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes);
        var isProviderTypeAndCourseTypesChangeInProgress = sessionModel != null;

        var selectedApprenticeshipUnits = SetSelectionMade(model);

        if (!isProviderTypeAndCourseTypesChangeInProgress)
        {
            var changeOfApprenticeshipUnits = ChangeOfApprenticeshipUnits(organisationResponse, selectedApprenticeshipUnits);
            if (!changeOfApprenticeshipUnits) return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
        }

        var result = _validator.Validate(model);

        if (!result.IsValid)
        {
            var viewModel = new ApprenticeshipUnitsUpdateViewModel
            {
                ApprenticeshipUnitsSelectionId = model.ApprenticeshipUnitsSelectionId,
                ApprenticeshipUnitsSelection = BuildApprenticeshipTypesChoices(model.ApprenticeshipUnitsSelectionId),
                OffersApprenticeships = model.OffersApprenticeships
            };
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View(viewModel);
        }

        string userDisplayName = User.UserDisplayName();

        if (!isProviderTypeAndCourseTypesChangeInProgress)
        {
            await UpdateCourseTypes(_outerApiClient, ukprn, organisationResponse.AllowedCourseTypes,
                selectedApprenticeshipUnits, userDisplayName, cancellationToken);
        }
        else
        {
            if (model.ApprenticeshipUnitsSelectionId is true)
            {
                sessionModel!.CourseTypeIds.Add(CourseTypes.ApprenticeshipUnitId);
            }

            await UpdateProviderTypeAndCourseTypes(_outerApiClient, _organisationPatchService, ukprn,
               userDisplayName, sessionModel!, organisationResponse, cancellationToken);
            _sessionService.Delete(SessionKeys.UpdateSupportingProviderCourseTypes);
        }

        return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
    }

    private static bool ChangeOfApprenticeshipUnits(GetOrganisationResponse organisationResponse, bool? selectedApprenticeshipUnits)
    {
        if (selectedApprenticeshipUnits == null) return true;
        var containsApprenticeshipUnits =
            organisationResponse.AllowedCourseTypes.Any(a => a.CourseTypeId == CourseTypes.ApprenticeshipUnitId);

        return selectedApprenticeshipUnits != containsApprenticeshipUnits;
    }

    private static async Task UpdateProviderTypeAndCourseTypes(IOuterApiClient client, IOrganisationPatchService patchService,
        int ukprn,
        string userDisplayName,
        UpdateProviderTypeCourseTypesSessionModel sessionModel,
        GetOrganisationResponse organisationResponse,
        CancellationToken cancellationToken)
    {
        var courseTypesPutModel = new UpdateCourseTypesModel(sessionModel!.CourseTypeIds, userDisplayName);
        await client.PutCourseTypes(ukprn, courseTypesPutModel, cancellationToken);

        PatchOrganisationModel patchModel = organisationResponse;
        patchModel.ProviderType = sessionModel!.ProviderType;
        await patchService.OrganisationPatched(ukprn, organisationResponse, patchModel, cancellationToken);
    }

    private static async Task UpdateCourseTypes(IOuterApiClient client, int ukprn,
        IEnumerable<AllowedCourseType> allowedCourseTypes, bool? selectedApprenticeshipUnits,
        string userDisplayName, CancellationToken cancellationToken)
    {
        var courseTypesModel = selectedApprenticeshipUnits!.Value
            ? AddShortCoursesToCourseTypes(allowedCourseTypes, userDisplayName)
            : RemoveShortCoursesFromCourseTypes(allowedCourseTypes, userDisplayName);

        await client.PutCourseTypes(ukprn, courseTypesModel, cancellationToken);
    }

    private static bool? SetSelectionMade(ApprenticeshipUnitsUpdateViewModel model)
    {
        bool? selectionMade = null;

        if (model.ApprenticeshipUnitsSelectionId.HasValue)
        {
            selectionMade = model.ApprenticeshipUnitsSelectionId;
        }

        return selectionMade;
    }

    private static ApprenticeshipUnitsUpdateViewModel BuildViewModelForProviderTypeAndCourseTypesJourney(List<int> courseTypeIds)
    {
        return new ApprenticeshipUnitsUpdateViewModel
        {
            ApprenticeshipUnitsSelectionId = null,
            ApprenticeshipUnitsSelection = BuildApprenticeshipTypesChoices(null),
            OffersApprenticeships = courseTypeIds.Any(c => c == CourseTypes.ApprenticeshipId)
        };
    }

    private static ApprenticeshipUnitsUpdateViewModel BuildViewModelFromCurrentCourseTypes(List<AllowedCourseType> allowedCourseTypes)
    {
        var containsApprenticeshipUnits =
            allowedCourseTypes.Any(a => a.CourseTypeId == CourseTypes.ApprenticeshipUnitId);

        var containsApprenticeships =
            allowedCourseTypes.Any(a => a.CourseTypeId == CourseTypes.ApprenticeshipId);

        var model = new ApprenticeshipUnitsUpdateViewModel
        {
            ApprenticeshipUnitsSelectionId = containsApprenticeshipUnits,
            ApprenticeshipUnitsSelection = BuildApprenticeshipTypesChoices(containsApprenticeshipUnits),
            OffersApprenticeships = containsApprenticeships
        };
        return model;
    }

    private static UpdateCourseTypesModel RemoveShortCoursesFromCourseTypes(IEnumerable<AllowedCourseType> courseTypes, string userDisplayName)
    {
        var courseTypeIdsToKeep = courseTypes.Where(x => x.CourseTypeId != CourseTypes.ApprenticeshipUnitId)
            .Select(a => a.CourseTypeId).ToList();
        var courseTypesModel = new UpdateCourseTypesModel(courseTypeIdsToKeep, userDisplayName);
        return courseTypesModel;
    }

    private static UpdateCourseTypesModel AddShortCoursesToCourseTypes(IEnumerable<AllowedCourseType> courseTypes, string userDisplayName)
    {
        var courseTypeIds = courseTypes.Select(a => a.CourseTypeId).ToList();
        courseTypeIds.Add(CourseTypes.ApprenticeshipUnitId);
        var courseTypesModel = new UpdateCourseTypesModel(courseTypeIds, userDisplayName);
        return courseTypesModel;
    }

    private static List<ApprenticeshipUnitsSelectionModel> BuildApprenticeshipTypesChoices(bool? selectedId)
    {
        return new List<ApprenticeshipUnitsSelectionModel>
        {
            new() { Description = "Yes", Id = true, IsSelected = selectedId is true},
            new() { Description = "No", Id = false, IsSelected = selectedId is false},
        };
    }
}

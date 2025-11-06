using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/apprenticeship-units", Name = RouteNames.ApprenticeshipUnitsUpdate)]
public class ApprenticeshipUnitsUpdateController(IOuterApiClient _outerApiClient, IValidator<ApprenticeshipUnitUpdateViewModel> _validator) : Controller
{
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn.ToString(), cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var containsApprenticeshipUnits =
            organisationResponse.AllowedCourseTypes.Any(a => a.CourseTypeId == CourseTypes.ApprenticeshipUnitId);

        var containsApprenticeships =
            organisationResponse.AllowedCourseTypes.Any(a => a.CourseTypeId == CourseTypes.ApprenticeshipId);

        var selectedId = containsApprenticeshipUnits ? 1 : 0;

        var model = new ApprenticeshipUnitUpdateViewModel
        {
            ApprenticeshipUnitsSelectionId = selectedId,
            ApprenticeshipUnitsSelection = BuildApprenticeshipTypesChoices(selectedId),
            OffersApprentices = containsApprenticeships
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(int ukprn, ApprenticeshipUnitUpdateViewModel model,
        CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn.ToString(), cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var containsApprenticeshipUnits =
            organisationResponse.AllowedCourseTypes.Any(a => a.CourseTypeId == CourseTypes.ApprenticeshipUnitId);

        var selectedApprenticeshipUnits = SetSelectionMade(model);

        if (selectedApprenticeshipUnits == containsApprenticeshipUnits)
        {
            return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
        }

        var result = _validator.Validate(model);

        if (!result.IsValid)
        {
            var viewModel = new ApprenticeshipUnitUpdateViewModel
            {
                ApprenticeshipUnitsSelectionId = model.ApprenticeshipUnitsSelectionId,
                ApprenticeshipUnitsSelection = BuildApprenticeshipTypesChoices(model.ApprenticeshipUnitsSelectionId),
                OffersApprentices = model.OffersApprentices
            };
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View(viewModel);
        }

        string userDisplayName = User.UserDisplayName();
        var courseTypesModel = selectedApprenticeshipUnits!.Value
            ? AddShortCoursesToCourseTypes(organisationResponse.AllowedCourseTypes, userDisplayName)
            : RemoveShortCoursesFromCourseTypes(organisationResponse.AllowedCourseTypes, userDisplayName);

        await _outerApiClient.PutCourseTypes(ukprn, courseTypesModel, cancellationToken);
        return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
    }

    private static bool? SetSelectionMade(ApprenticeshipUnitUpdateViewModel model)
    {
        bool? selectionMade = null;

        switch (model.ApprenticeshipUnitsSelectionId)
        {
            case 0:
                selectionMade = false;
                break;
            case 1:
                selectionMade = true;
                break;
        }

        return selectionMade;
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

    private static List<ApprenticeshipUnitsSelectionModel> BuildApprenticeshipTypesChoices(int? selectedId)
    {
        return new List<ApprenticeshipUnitsSelectionModel>
        {
            new() { Description = "Yes", Id = 1, IsSelected = selectedId is 1},
            new() { Description = "No", Id = 0, IsSelected = selectedId is 0},
        };
    }
}

using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("providers/{ukprn}/apprenticeship-units", Name = RouteNames.ApprenticeshipUnitsUpdate)]

public class ApprenticeshipUnitsUpdateController(IOuterApiClient _outerApiClient) : Controller
{
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn.ToString(), cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var containsApprenticeshipUnits =
            organisationResponse.AllowedCourseTypes.Any(a => a.LearningType == LearningType.ShortCourse);

        var model = new ApprenticeshipUnitUpdateViewModel
        {
            ApprenticeshipUnitSelectionChoice = containsApprenticeshipUnits,
            ApprenticeshipUnitsSelection = BuildApprenticeshipTypesChoices(containsApprenticeshipUnits)
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

        if (model.ApprenticeshipUnitSelectionChoice == containsApprenticeshipUnits)
        {
            return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
        }

        string userDisplayName = User.UserDisplayName();
        var courseTypesModel = model.ApprenticeshipUnitSelectionChoice
            ? AddShortCoursesToCourseTypes(organisationResponse.AllowedCourseTypes, userDisplayName)
            : RemoveShortCoursesFromCourseTypes(organisationResponse.AllowedCourseTypes, userDisplayName);

        await _outerApiClient.PutCourseTypes(ukprn, courseTypesModel, cancellationToken);
        return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
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

    private static List<ApprenticeshipUnitsSelectionModel> BuildApprenticeshipTypesChoices(bool containsApprenticeshipUnits)
    {
        return new List<ApprenticeshipUnitsSelectionModel>
        {
            new() { Description = "Yes", Id = true, IsSelected = containsApprenticeshipUnits},
            new() { Description = "No", Id = false, IsSelected = !containsApprenticeshipUnits},
        };
    }
}

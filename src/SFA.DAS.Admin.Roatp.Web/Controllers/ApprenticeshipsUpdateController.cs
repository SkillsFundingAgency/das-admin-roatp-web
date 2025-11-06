using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("providers/{ukprn}/apprenticeships", Name = RouteNames.ApprenticeshipsUpdate)]

public class ApprenticeshipsUpdateController(IOuterApiClient _outerApiClient, IHttpContextAccessor _contextAccessor, ISessionService _sessionService) : Controller
{
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn.ToString(), cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);


        var sessionModel = _sessionService.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes);
        if (sessionModel == null) return RedirectToRoute(RouteNames.Home);



        bool? containsApprenticeships = null;
        if (sessionModel.CourseTypes.Any())
        {
            containsApprenticeships = sessionModel.CourseTypes.Any(a => a.LearningType == LearningType.Standard);
        }

        var model = new ApprenticeshipTypeUpdateViewModel
        {
            ApprenticeshipUnitSelectionChoice = containsApprenticeships,
            ApprenticeshipUnitsSelection = BuildApprenticeshipsChoices(containsApprenticeships)
        };

        return View(model);
    }

    // [HttpPost]
    // public async Task<IActionResult> Index(int ukprn, ApprenticeshipTypeUpdateViewModel model,
    //     CancellationToken cancellationToken)
    // {
    //     var organisationResponse = await _outerApiClient.GetOrganisation(ukprn.ToString(), cancellationToken);
    //
    //     if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);
    //
    //     var containsApprenticeshipUnits =
    //         organisationResponse.AllowedCourseTypes.Any(a => a.LearningType == LearningType.ShortCourse);
    //
    //     if (model.ApprenticeshipUnitSelectionChoice == containsApprenticeshipUnits)
    //     {
    //         return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
    //     }
    //
    //     string userDisplayName = _contextAccessor.HttpContext!.User.UserDisplayName();
    //     var courseTypesModel = model.ApprenticeshipUnitSelectionChoice
    //         ? AddShortCoursesToCourseTypes(organisationResponse.AllowedCourseTypes, userDisplayName)
    //         : RemoveShortCoursesFromCourseTypes(organisationResponse.AllowedCourseTypes, userDisplayName);
    //
    //     await _outerApiClient.PutCourseTypes(ukprn, courseTypesModel, cancellationToken);
    //     return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
    // }
    //
    // private static UpdateCourseTypesModel RemoveShortCoursesFromCourseTypes(IEnumerable<AllowedCourseType> courseTypes, string userDisplayName)
    // {
    //     var currentCourseTypeIds = courseTypes.Where(x => x.LearningType != LearningType.ShortCourse)
    //         .Select(a => (int)a.LearningType).ToList();
    //     var courseTypesModel = new UpdateCourseTypesModel(currentCourseTypeIds.Distinct().ToList(), userDisplayName);
    //     return courseTypesModel;
    // }
    //
    // private static UpdateCourseTypesModel AddShortCoursesToCourseTypes(IEnumerable<AllowedCourseType> courseTypes, string userDisplayName)
    // {
    //     var currentCourseTypeIds = courseTypes.Select(a => (int)a.LearningType).ToList();
    //     currentCourseTypeIds.Add((int)LearningType.ShortCourse);
    //     var courseTypesModel = new UpdateCourseTypesModel(currentCourseTypeIds.Distinct().ToList(), userDisplayName);
    //     return courseTypesModel;
    // }
    //
    private static List<ApprenticeshipUnitsSelectionModel> BuildApprenticeshipsChoices(bool? containsApprenticeshipUnits)
    {
        var selectionModel = new List<ApprenticeshipUnitsSelectionModel>();

        if (!containsApprenticeshipUnits.HasValue)
        {
            return new List<ApprenticeshipUnitsSelectionModel>
            {
                new() { Description = "Yes", Id = true},
                new() { Description = "No", Id = false},
            };
        }

        return new List<ApprenticeshipUnitsSelectionModel>
            {
                new() { Description = "Yes", Id = true, IsSelected = containsApprenticeshipUnits.Value},
                new() { Description = "No", Id = false, IsSelected =  !containsApprenticeshipUnits.Value}
            };
    }
}

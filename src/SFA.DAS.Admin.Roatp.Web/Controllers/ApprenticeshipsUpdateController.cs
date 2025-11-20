using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("providers/{ukprn}/apprenticeships", Name = RouteNames.ApprenticeshipsUpdate)]

public class ApprenticeshipsUpdateController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IValidator<ApprenticeshipsUpdateViewModel> _validator) : Controller
{
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var sessionModel = _sessionService.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes);
        if (sessionModel == null) return RedirectToRoute(RouteNames.Home);

        bool? containsApprenticeships = null;
        if (sessionModel.CourseTypeIds.Count > 0)
        {
            containsApprenticeships = sessionModel.CourseTypeIds.Any(a => a == (int)LearningType.Standard);
        }

        var model = new ApprenticeshipsUpdateViewModel
        {
            ApprenticeshipsSelectionChoice = containsApprenticeships,
            ApprenticeshipsSelection = BuildApprenticeshipsChoices(containsApprenticeships)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(int ukprn, ApprenticeshipsUpdateViewModel model,
        CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var result = _validator.Validate(model);

        if (!result.IsValid)
        {
            var viewModel = new ApprenticeshipsUpdateViewModel
            {
                ApprenticeshipsSelectionChoice = model.ApprenticeshipsSelectionChoice,
                ApprenticeshipsSelection = BuildApprenticeshipsChoices(model.ApprenticeshipsSelectionChoice)
            };

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View(viewModel);
        }

        if (model.ApprenticeshipsSelectionChoice is true)
        {
            var sessionModel =
                _sessionService.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys
                    .UpdateSupportingProviderCourseTypes);
            sessionModel.CourseTypeIds = new List<int> { CourseTypes.ApprenticeshipId };
            _sessionService.Set<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes, sessionModel);
        }

        return RedirectToRoute(RouteNames.ApprenticeshipUnitsUpdate, new { ukprn });
    }

    private static List<ApprenticeshipsSelectionModel> BuildApprenticeshipsChoices(bool? containsApprenticeshipUnits)
    {
        return new List<ApprenticeshipsSelectionModel>
            {
                new() { Description = "Yes", Id = true, IsSelected = containsApprenticeshipUnits is true},
                new() { Description = "No", Id = false, IsSelected =  containsApprenticeshipUnits is false}
            };
    }
}

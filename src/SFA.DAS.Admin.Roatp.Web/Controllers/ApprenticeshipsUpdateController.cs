using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("providers/{ukprn}/apprenticeships", Name = RouteNames.ApprenticeshipsUpdate)]

public class ApprenticeshipsUpdateController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IValidator<OfferApprenticeshipsSubmitModel> _validator) : Controller
{
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var organisationApiResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationApiResponse.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        var sessionModel = _sessionService.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes);
        if (sessionModel == null) return RedirectToRoute(RouteNames.Home);

        bool? containsApprenticeships = null;
        if (sessionModel.CourseTypeIds.Count > 0)
        {
            containsApprenticeships = sessionModel.CourseTypeIds.Any(a => a == CourseTypes.Apprenticeship);
        }

        var model = new OfferApprenticeshipsViewModel
        {
            IsApprenticeshipsOffered = containsApprenticeships,
            ApprenticeshipsSelection = BuildApprenticeshipsChoices(containsApprenticeships)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(int ukprn, OfferApprenticeshipsSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var organisationApiResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);

        if (organisationApiResponse.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var viewModel = new OfferApprenticeshipsViewModel
            {
                IsApprenticeshipsOffered = submitModel.IsApprenticeshipsOffered,
                ApprenticeshipsSelection = BuildApprenticeshipsChoices(submitModel.IsApprenticeshipsOffered)
            };

            ModelState.AddValidationErrors(result.Errors);

            return View(viewModel);
        }

        if (submitModel.IsApprenticeshipsOffered is true)
        {
            var sessionModel =
                _sessionService.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys
                    .UpdateSupportingProviderCourseTypes);
            sessionModel.CourseTypeIds = new List<int> { CourseTypes.Apprenticeship };
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

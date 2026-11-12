using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("providers/{ukprn}/apprenticeships", Name = RouteNames.ApprenticeshipsUpdate)]
public class ApprenticeshipsUpdateController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IValidator<OfferApprenticeshipsSubmitModel> _validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var organisationApiResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);
        if (organisationApiResponse.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        var organisationResponse = organisationApiResponse.Content!;
        var sessionModel = _sessionService.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes);
        if (sessionModel is not null)
        {
            bool? containsApprenticeships = null;
            if (sessionModel.CourseTypeIds.Count > 0)
            {
                containsApprenticeships = sessionModel.CourseTypeIds.Any(a => a == (int)CourseType.Apprenticeship);
            }

            return View(BuildViewModel(containsApprenticeships));
        }

        if (organisationResponse.ProviderType != ProviderType.Employer)
        {
            return RedirectToRoute(RouteNames.Home);
        }

        if (OffersApprenticeships(organisationResponse))
        {
            return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
        }

        return View(BuildViewModel(false));
    }

    [HttpPost]
    public async Task<IActionResult> Index(int ukprn, OfferApprenticeshipsSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var result = _validator.Validate(submitModel);
        if (!result.IsValid)
        {
            ModelState.AddValidationErrors(result.Errors);
            return View(BuildViewModel(submitModel.IsApprenticeshipsOffered));
        }

        var sessionModel = _sessionService.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes);
        if (sessionModel is not null)
        {
            if (submitModel.IsApprenticeshipsOffered is true)
            {
                sessionModel.CourseTypeIds = [(int)CourseType.Apprenticeship];
                _sessionService.Set(SessionKeys.UpdateSupportingProviderCourseTypes, sessionModel);
            }

            return RedirectToRoute(RouteNames.ApprenticeshipUnitsUpdate, new { ukprn });
        }

        var organisationApiResponse = await _outerApiClient.GetOrganisation(ukprn, cancellationToken);
        if (organisationApiResponse.StatusCode != HttpStatusCode.OK) return RedirectToRoute(RouteNames.Home);

        var organisationResponse = organisationApiResponse.Content!;
        if (organisationResponse.ProviderType != ProviderType.Employer)
        {
            return RedirectToRoute(RouteNames.Home);
        }

        if (!OffersApprenticeships(organisationResponse) && submitModel.IsApprenticeshipsOffered is true)
        {
            UpdateCourseTypesModel updateCourseTypesModel = BuildCourseTypesModel(
                offersApprenticeships: true,
                offersApprenticeshipUnits: OffersApprenticeshipUnits(organisationResponse),
                User.UserId(),
                User.UserDisplayName());
            await _outerApiClient.PutCourseTypes(ukprn, updateCourseTypesModel, cancellationToken);
        }

        return RedirectToRoute(RouteNames.ProviderSummary, new { ukprn });
    }

    private static bool OffersApprenticeships(GetOrganisationResponse organisationResponse)
        => organisationResponse.AllowedCourseTypes.Any(x => x.CourseType == CourseType.Apprenticeship);

    private static bool OffersApprenticeshipUnits(GetOrganisationResponse organisationResponse)
        => organisationResponse.AllowedCourseTypes.Any(x => x.CourseType == CourseType.ShortCourse);

    private static OfferApprenticeshipsViewModel BuildViewModel(bool? containsApprenticeships)
        => new()
        {
            IsApprenticeshipsOffered = containsApprenticeships,
            ApprenticeshipsSelection = BuildApprenticeshipsChoices(containsApprenticeships)
        };

    private static UpdateCourseTypesModel BuildCourseTypesModel(
        bool offersApprenticeships,
        bool offersApprenticeshipUnits,
        string userId,
        string userDisplayName)
    {
        var courseTypeIds = new List<int>();
        if (offersApprenticeships)
        {
            courseTypeIds.Add((int)CourseType.Apprenticeship);
        }

        if (offersApprenticeshipUnits)
        {
            courseTypeIds.Add((int)CourseType.ShortCourse);
        }

        return new UpdateCourseTypesModel(courseTypeIds, userId, userDisplayName);
    }

    private static List<ApprenticeshipsSelectionModel> BuildApprenticeshipsChoices(bool? containsApprenticeships)
    {
        return
        [
            new() { Description = "Yes", Id = true, IsSelected = containsApprenticeships is true },
            new() { Description = "No", Id = false, IsSelected = containsApprenticeships is false }
        ];
    }
}

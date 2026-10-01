using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses/{larsCode}/change-restriction", Name = RouteNames.RestrictedCourseChangeRestriction)]
public class RestrictedCourseChangeRestrictionController(
    ISessionService sessionService,
    IOuterApiClient outerApiClient,
    IValidator<RestrictedCourseChangeRestrictionSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/RestrictedCourseChangeRestriction/Index.cshtml";

    [HttpGet]
    public Task<IActionResult> Index(int ukprn, string larsCode, CancellationToken cancellationToken = default)
        => HandleChangeRestriction(ukprn, larsCode, submitModel: null, cancellationToken);

    [HttpPost]
    public Task<IActionResult> Index(
        int ukprn,
        string larsCode,
        RestrictedCourseChangeRestrictionSubmitModel submitModel,
        CancellationToken cancellationToken = default)
        => HandleChangeRestriction(ukprn, larsCode, submitModel, cancellationToken);

    private async Task<IActionResult> HandleChangeRestriction(
        int ukprn,
        string larsCode,
        RestrictedCourseChangeRestrictionSubmitModel? submitModel,
        CancellationToken cancellationToken)
    {
        var restrictedApprenticeship = await GetRestrictedApprenticeship(ukprn, larsCode, cancellationToken);
        if (restrictedApprenticeship is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        var providerName = await sessionService.GetProviderName(outerApiClient, ukprn, cancellationToken);
        if (providerName is null)
        {
            return NotFound();
        }

        var courseDisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(restrictedApprenticeship.Title, restrictedApprenticeship.Level);

        SetChangeSession(ukprn, restrictedApprenticeship, providerName, courseDisplayTitle);

        var model = BuildViewModel(ukprn, restrictedApprenticeship, courseDisplayTitle, submitModel);
        if (submitModel is null)
        {
            return View(ViewPath, model);
        }

        var validationResult = validator.Validate(submitModel);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, model);
        }

        if (submitModel.SelectedOption == RestrictedCourseChangeRestrictionOptions.AddOrChange)
        {
            return RedirectToRoute(RouteNames.ChangeRestrictedCourseLastStartDate, new { ukprn, larsCode });
        }

        return View(ViewPath, model);
    }

    private void SetChangeSession(
        int ukprn,
        ProviderRestrictedApprenticeshipModel restrictedApprenticeship,
        string providerName,
        string courseDisplayTitle)
    {
        sessionService.Set(SessionKeys.ProviderRestrictedCourseChangeRestriction, new ChangeRestrictedCourseRestrictionSessionModel
        {
            Ukprn = ukprn,
            LarsCode = restrictedApprenticeship.LarsCode,
            CourseDisplayTitle = courseDisplayTitle,
            ProviderName = providerName,
            LastDateStarts = restrictedApprenticeship.LastDateStarts
        });
    }

    private RestrictedCourseChangeRestrictionViewModel BuildViewModel(
        int ukprn,
        ProviderRestrictedApprenticeshipModel restrictedApprenticeship,
        string courseDisplayTitle,
        RestrictedCourseChangeRestrictionSubmitModel? submitModel)
        => new()
        {
            Ukprn = ukprn,
            LarsCode = restrictedApprenticeship.LarsCode,
            CourseDisplayTitle = courseDisplayTitle,
            LastDateStarts = restrictedApprenticeship.LastDateStarts,
            SelectedOption = submitModel?.SelectedOption,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderRestrictedApprenticeships, new { ukprn })!
        };

    private async Task<ProviderRestrictedApprenticeshipModel?> GetRestrictedApprenticeship(
        int ukprn,
        string larsCode,
        CancellationToken cancellationToken)
    {
        var apiResponse = await outerApiClient.GetRestrictedApprenticeships(ukprn, cancellationToken);
        if (apiResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await apiResponse.EnsureSuccessfulAsync();
        return apiResponse.Content?.Courses?.FirstOrDefault(course => course.LarsCode == larsCode);
    }
}

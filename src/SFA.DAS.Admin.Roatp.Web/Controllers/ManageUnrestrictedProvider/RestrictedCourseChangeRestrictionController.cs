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
    public async Task<IActionResult> Index(int ukprn, string larsCode, CancellationToken cancellationToken = default)
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

        sessionService.Set(SessionKeys.RestrictedCourseChangeRestriction, new ChangeRestrictedCourseRestrictionSessionModel
        {
            Ukprn = ukprn,
            LarsCode = restrictedApprenticeship.LarsCode,
            CourseDisplayTitle = courseDisplayTitle,
            ProviderName = providerName,
            CourseLastDateStarts = restrictedApprenticeship.LastDateStarts
        });

        return View(ViewPath, BuildViewModel(
            ukprn,
            restrictedApprenticeship.LarsCode,
            courseDisplayTitle,
            restrictedApprenticeship.LastDateStarts));
    }

    [HttpPost]
    public IActionResult Index(
        int ukprn,
        string larsCode,
        RestrictedCourseChangeRestrictionSubmitModel submitModel)
    {
        var session = GetChangeRestrictedCourseSession(ukprn, larsCode);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        var model = BuildViewModel(
            ukprn,
            session.LarsCode,
            session.CourseDisplayTitle,
            session.CourseLastDateStarts,
            submitModel);

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

    private ChangeRestrictedCourseRestrictionSessionModel? GetChangeRestrictedCourseSession(int ukprn, string larsCode)
    {
        var session = sessionService.Get<ChangeRestrictedCourseRestrictionSessionModel>(
            SessionKeys.RestrictedCourseChangeRestriction);
        if (session is null || session.Ukprn != ukprn || session.LarsCode != larsCode)
        {
            return null;
        }

        return session;
    }

    private RestrictedCourseChangeRestrictionViewModel BuildViewModel(
        int ukprn,
        string larsCode,
        string courseDisplayTitle,
        DateTime? lastDateStarts,
        RestrictedCourseChangeRestrictionSubmitModel? submitModel = null)
        => new()
        {
            Ukprn = ukprn,
            LarsCode = larsCode,
            CourseDisplayTitle = courseDisplayTitle,
            LastDateStarts = lastDateStarts,
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

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses/add/set-last-start-date", Name = RouteNames.AddRestrictedCourseLastStartDate)]
public class AddRestrictedCourseLastStartDateController(
    ISessionService sessionService,
    IOuterApiClient outerApiClient,
    IValidator<SetLastDateStartsSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/AddRestrictedCourseLastStartDate/Index.cshtml";

    public static string GetSuccessBannerMessage(string displayTitle) =>
        $"{displayTitle} is now restricted and has a last date for new starts";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken = default)
    {
        var session = GetProviderRestrictedCourseSession(ukprn);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        var courseDetails = await outerApiClient.GetCourseDetails(session.LarsCode, cancellationToken);
        session.CourseLastDateStarts = courseDetails?.LastDateStarts;
        sessionService.Set(SessionKeys.ProviderRestrictedCourse, session);

        var model = BuildViewModel(session);
        return View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        int ukprn,
        SetLastDateStartsSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var session = GetProviderRestrictedCourseSession(ukprn);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        submitModel.LarsCode = session.LarsCode;
        submitModel.CourseLastDateStarts = session.CourseLastDateStarts;

        var validationResult = await validator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            var model = BuildViewModel(session, submitModel.Day, submitModel.Month, submitModel.Year);
            return View(ViewPath, model);
        }

        var lastDateStarts = submitModel.GetEnteredDate();

        var response = await outerApiClient.UpsertProviderAllowedCourse(
            ukprn,
            session.LarsCode,
            new UpsertProviderAllowedCourseRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName(),
                LastDateStarts = lastDateStarts
            },
            cancellationToken);

        await response.EnsureSuccessStatusCodeAsync();

        sessionService.Delete(SessionKeys.ProviderRestrictedCourse);

        TempData[ProviderRestrictedApprenticeshipsController.SuccessBannerTempDataKey] =
            GetSuccessBannerMessage(session.CourseDisplayTitle);

        return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
    }

    private ProviderRestrictedCourseSessionModel? GetProviderRestrictedCourseSession(int ukprn)
    {
        var session = sessionService.Get<ProviderRestrictedCourseSessionModel>(SessionKeys.ProviderRestrictedCourse);
        if (session is null || session.Ukprn != ukprn)
        {
            return null;
        }

        return session;
    }

    private AddRestrictedCourseLastStartDateViewModel BuildViewModel(
        ProviderRestrictedCourseSessionModel session,
        string? day = null,
        string? month = null,
        string? year = null)
    {
        return new AddRestrictedCourseLastStartDateViewModel
        {
            Ukprn = session.Ukprn,
            ProviderName = session.ProviderName,
            CourseDisplayTitle = session.CourseDisplayTitle,
            LarsCode = session.LarsCode,
            CourseLastDateStarts = session.CourseLastDateStarts,
            Day = day,
            Month = month,
            Year = year,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderRestrictedApprenticeships, new { ukprn = session.Ukprn })!
        };
    }
}

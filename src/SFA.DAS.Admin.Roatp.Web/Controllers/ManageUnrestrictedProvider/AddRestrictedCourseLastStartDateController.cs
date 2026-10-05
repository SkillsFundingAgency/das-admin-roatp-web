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

        var model = await BuildViewModelAsync(session, null, cancellationToken);
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

        var model = await BuildViewModelAsync(session, submitModel, cancellationToken);

        submitModel.LarsCode = session.LarsCode;
        submitModel.CourseLastDateStarts = model.CourseLastDateStarts;

        var validationResult = await validator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, model);
        }

        submitModel.TryGetEnteredDate(out var lastDateStarts);

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

    private async Task<AddRestrictedCourseLastStartDateViewModel> BuildViewModelAsync(
        ProviderRestrictedCourseSessionModel session,
        SetLastDateStartsSubmitModel? submitModel,
        CancellationToken cancellationToken)
    {
        var courseDetails = await outerApiClient.GetCourseDetails(session.LarsCode, cancellationToken);

        return new AddRestrictedCourseLastStartDateViewModel
        {
            Ukprn = session.Ukprn,
            ProviderName = session.ProviderName,
            CourseDisplayTitle = session.CourseDisplayTitle,
            LarsCode = session.LarsCode,
            CourseLastDateStarts = courseDetails?.LastDateStarts,
            Day = submitModel?.Day,
            Month = submitModel?.Month,
            Year = submitModel?.Year,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderRestrictedApprenticeships, new { ukprn = session.Ukprn })!
        };
    }
}

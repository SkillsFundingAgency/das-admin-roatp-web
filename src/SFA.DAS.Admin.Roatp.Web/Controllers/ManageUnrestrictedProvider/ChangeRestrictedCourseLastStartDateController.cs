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
[Route("providers/{ukprn}/restricted-courses/{larsCode}/change-restriction/set-last-start-date",
    Name = RouteNames.ChangeRestrictedCourseLastStartDate)]
public class ChangeRestrictedCourseLastStartDateController(
    ISessionService sessionService,
    IOuterApiClient outerApiClient,
    IValidator<SetLastDateStartsSubmitModel> validator,
    IValidator<ChangeRestrictedCourseRestrictionSessionModel> validatorForLastStartDateChange) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/ChangeRestrictedCourseLastStartDate/Index.cshtml";

    public static string GetSuccessBannerMessage(string displayTitle) =>
        $"{displayTitle} last start date has been updated";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, string larsCode, CancellationToken cancellationToken = default)
    {
        var session = GetChangeRestrictedCourseSession(ukprn, larsCode);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        if (!await IsValidForLastStartDateChange(session, cancellationToken))
        {
            return NotFound();
        }

        var model = await BuildViewModel(session, submitModel: null, cancellationToken);
        return View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        int ukprn,
        string larsCode,
        SetLastDateStartsSubmitModel submitModel,
        CancellationToken cancellationToken = default)
    {
        var session = GetChangeRestrictedCourseSession(ukprn, larsCode);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        var model = await BuildViewModel(session, submitModel, cancellationToken);

        submitModel.LarsCode = larsCode;
        submitModel.CourseLastDateStarts = model.CourseLastDateStarts;

        var validationResult = await validator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, model);
        }

        submitModel.TryGetEnteredDate(out var lastDateStarts);

        await ChangeRestrictedApprenticeshipLastDateStarts(ukprn, larsCode, lastDateStarts, cancellationToken);

        sessionService.Delete(SessionKeys.RestrictedCourseChangeRestriction);

        TempData[ProviderRestrictedApprenticeshipsController.SuccessBannerTempDataKey] =
            GetSuccessBannerMessage(session.CourseDisplayTitle);

        return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
    }

    private async Task ChangeRestrictedApprenticeshipLastDateStarts(
        int ukprn,
        string larsCode,
        DateTime lastDateStarts,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.ChangeRestrictedApprenticeshipLastDateStarts(
            ukprn,
            larsCode,
            new ChangeRestrictedApprenticeshipLastDateStartsRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName(),
                LastDateStarts = lastDateStarts
            },
            cancellationToken);

        await response.EnsureSuccessStatusCodeAsync();
    }

    private async Task<bool> IsValidForLastStartDateChange(
        ChangeRestrictedCourseRestrictionSessionModel session,
        CancellationToken cancellationToken)
    {
        var validateResult = await validatorForLastStartDateChange.ValidateAsync(session, cancellationToken);

        return validateResult.IsValid;
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

    private async Task<ChangeRestrictedCourseLastStartDateViewModel> BuildViewModel(
        ChangeRestrictedCourseRestrictionSessionModel session,
        SetLastDateStartsSubmitModel? submitModel,
        CancellationToken cancellationToken)
    {
        var courseDetails = await outerApiClient.GetCourseDetails(session.LarsCode, cancellationToken);

        var day = submitModel?.Day;
        var month = submitModel?.Month;
        var year = submitModel?.Year;

        if (submitModel is null && session.CourseLastDateStarts.HasValue)
        {
            var existingLastDateStarts = session.CourseLastDateStarts.Value;
            day = existingLastDateStarts.Day.ToString("00");
            month = existingLastDateStarts.Month.ToString("00");
            year = existingLastDateStarts.Year.ToString();
        }

        return new ChangeRestrictedCourseLastStartDateViewModel
        {
            Ukprn = session.Ukprn,
            LarsCode = session.LarsCode,
            CourseDisplayTitle = session.CourseDisplayTitle,
            CourseLastDateStarts = courseDetails?.LastDateStarts,
            Day = day,
            Month = month,
            Year = year,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderRestrictedApprenticeships, new { ukprn = session.Ukprn })!
        };
    }
}

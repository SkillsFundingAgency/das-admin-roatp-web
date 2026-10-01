using System.Net;
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
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses/{larsCode}/change-restriction/set-last-start-date",
    Name = RouteNames.ChangeRestrictedCourseLastStartDate)]
public class ChangeRestrictedCourseLastStartDateController(
    ISessionService sessionService,
    IOuterApiClient outerApiClient,
    IValidator<SetLastDateStartsSubmitModel> validator,
    IValidator<ChangeRestrictedCourseLastStartDateModel> validatorForLastStartDateChange) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/ChangeRestrictedCourseLastStartDate/Index.cshtml";

    public static string GetSuccessBannerMessage(string displayTitle) =>
        $"{displayTitle} last start date has been updated";

    [HttpGet]
    public Task<IActionResult> Index(int ukprn, string larsCode, CancellationToken cancellationToken = default)
        => HandleChangeLastStartDate(ukprn, larsCode, submitModel: null, cancellationToken);

    [HttpPost]
    public Task<IActionResult> Index(
        int ukprn,
        string larsCode,
        SetLastDateStartsSubmitModel submitModel,
        CancellationToken cancellationToken = default)
        => HandleChangeLastStartDate(ukprn, larsCode, submitModel, cancellationToken);

    private async Task<IActionResult> HandleChangeLastStartDate(
        int ukprn,
        string larsCode,
        SetLastDateStartsSubmitModel? submitModel,
        CancellationToken cancellationToken)
    {
        var session = GetSession(ukprn, larsCode);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        if (submitModel is null && !await IsValidForLastStartDateChange(session, cancellationToken))
        {
            return NotFound();
        }

        var model = await BuildViewModel(session, submitModel, cancellationToken);
        if (submitModel is null)
        {
            return View(ViewPath, model);
        }

        submitModel.LarsCode = larsCode;
        submitModel.CourseLastDateStarts = model.CourseLastDateStarts;

        var validationResult = await validator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, model);
        }

        if (!submitModel.TryGetEnteredDate(out var lastDateStarts))
        {
            ModelState.AddModelError(
                SetLastDateStartsSubmitModelValidator.DateFieldName,
                SetLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage);
            return View(ViewPath, model);
        }

        await ChangeRestrictedApprenticeshipLastDateStarts(ukprn, larsCode, lastDateStarts, cancellationToken);

        sessionService.Delete(SessionKeys.ProviderRestrictedCourseChangeRestriction);

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
        var validateResult = await validatorForLastStartDateChange.ValidateAsync(
            new ChangeRestrictedCourseLastStartDateModel
            {
                Ukprn = session.Ukprn,
                LarsCode = session.LarsCode,
                LastDateStarts = session.LastDateStarts
            },
            cancellationToken);

        return validateResult.IsValid;
    }

    private ChangeRestrictedCourseRestrictionSessionModel? GetSession(int ukprn, string larsCode)
    {
        var session = sessionService.Get<ChangeRestrictedCourseRestrictionSessionModel>(
            SessionKeys.ProviderRestrictedCourseChangeRestriction);
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
        var day = submitModel?.Day;
        var month = submitModel?.Month;
        var year = submitModel?.Year;

        if (submitModel is null && session.LastDateStarts.HasValue)
        {
            var existingLastDateStarts = session.LastDateStarts.Value;
            day = existingLastDateStarts.Day.ToString("00");
            month = existingLastDateStarts.Month.ToString("00");
            year = existingLastDateStarts.Year.ToString();
        }

        return new ChangeRestrictedCourseLastStartDateViewModel
        {
            Ukprn = session.Ukprn,
            LarsCode = session.LarsCode,
            CourseDisplayTitle = session.CourseDisplayTitle,
            CourseLastDateStarts = await GetCourseLastDateStarts(session.LarsCode, cancellationToken),
            Day = day,
            Month = month,
            Year = year,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderRestrictedApprenticeships, new { ukprn = session.Ukprn })!
        };
    }

    private async Task<DateTime?> GetCourseLastDateStarts(string larsCode, CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetAllowedProvidersForCourse(larsCode, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync();
        return response.Content?.LastDateStarts;
    }
}

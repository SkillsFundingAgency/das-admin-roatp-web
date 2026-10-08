using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.CourseRestrictions;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}/providers/{ukprn}/set-last-start-date", Name = RouteNames.SetLastDateStarts)]
public class SetLastDateStartsController(
    IOuterApiClient outerApiClient,
    ISessionService sessionService,
    IValidator<SetLastDateStartsSubmitModel> setLastDateStartsValidator) : Controller
{
    public const string ViewPath = "~/Views/CourseRestrictions/SetLastDateStarts/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        [FromRoute] int ukprn,
        CancellationToken cancellationToken)
    {
        var courseDetails = await outerApiClient.GetCourseDetails(larsCode, cancellationToken);
        var provider = courseDetails?.Providers.FirstOrDefault(p => p.Ukprn == ukprn);
        if (courseDetails is null || provider is null)
        {
            return NotFound();
        }

        var session = new SetLastDateStartsSessionModel
        {
            Ukprn = ukprn,
            LarsCode = larsCode,
            ProviderName = provider.ProviderName,
            CourseDisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(courseDetails.CourseName, courseDetails.Level),
            CourseLastDateStarts = courseDetails.LastDateStarts,
            ProviderLastDateStarts = provider.LastDateStarts
        };
        sessionService.Set(SessionKeys.SetLastDateStarts, session);

        string? day = null;
        string? month = null;
        string? year = null;
        if (session.ProviderLastDateStarts.HasValue)
        {
            var existingLastDateStarts = session.ProviderLastDateStarts.Value;
            day = existingLastDateStarts.Day.ToString("00");
            month = existingLastDateStarts.Month.ToString("00");
            year = existingLastDateStarts.Year.ToString();
        }

        return View(ViewPath, BuildViewModel(session, day, month, year));
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        [FromRoute] int ukprn,
        SetLastDateStartsSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var session = GetSession(ukprn, larsCode);
        if (session is null)
        {
            return NotFound();
        }

        submitModel.LarsCode = larsCode;
        submitModel.CourseLastDateStarts = session.CourseLastDateStarts;
        var validationResult = await setLastDateStartsValidator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, BuildViewModel(session, submitModel.Day, submitModel.Month, submitModel.Year));
        }

        var lastDateStarts = submitModel.GetEnteredDate();

        var response = await outerApiClient.PatchProviderAllowedCourse(
             ukprn,
             larsCode,
             User.UserId(),
             User.UserDisplayName(),
             new PatchProviderAllowedCourseRequest { LastDateStarts = lastDateStarts },
             cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        await response.EnsureSuccessStatusCodeAsync();

        sessionService.Delete(SessionKeys.SetLastDateStarts);

        TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey] = session.ProviderLastDateStarts.HasValue
            ? $"{session.ProviderName} last start date has been updated"
            : $"Last start date added for {session.ProviderName}";

        return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
    }

    private SetLastDateStartsSessionModel? GetSession(int ukprn, string larsCode)
    {
        var model = sessionService.Get<SetLastDateStartsSessionModel>(SessionKeys.SetLastDateStarts);
        if (model is null || model.Ukprn != ukprn || model.LarsCode != larsCode)
        {
            return null;
        }

        return model;
    }

    private SetLastDateStartsViewModel BuildViewModel(
        SetLastDateStartsSessionModel session,
        string? day,
        string? month,
        string? year)
    {
        return new SetLastDateStartsViewModel
        {
            LarsCode = session.LarsCode,
            Ukprn = session.Ukprn,
            ProviderName = session.ProviderName,
            CourseDisplayTitle = session.CourseDisplayTitle,
            Day = day,
            Month = month,
            Year = year,
            CourseLastDateStarts = session.CourseLastDateStarts,
            IsChangingExistingDate = session.ProviderLastDateStarts.HasValue,
            CancelUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode = session.LarsCode })!
        };
    }
}

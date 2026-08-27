using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}/providers/{ukprn}/set-last-start-date", Name = RouteNames.SetLastDateStarts)]
public class SetLastDateStartsController(
    IRestrictedCourseProviderService restrictedCourseProviderService,
    IOuterApiClient outerApiClient,
    IValidator<SetLastDateStartsSubmitModel> setDateValidator) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/SetLastDateStarts/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        [FromRoute] int ukprn,
        CancellationToken cancellationToken)
    {
        if (!await restrictedCourseProviderService.IsRouteValidAsync(larsCode, ukprn, cancellationToken))
        {
            return NotFound();
        }

        var model = await BuildViewModelAsync(larsCode, ukprn, null, cancellationToken);
        return model is null ? NotFound() : View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        [FromRoute] int ukprn,
        SetLastDateStartsSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        if (!await restrictedCourseProviderService.IsRouteValidAsync(larsCode, ukprn, cancellationToken))
        {
            return NotFound();
        }

        submitModel.LarsCode = larsCode;
        var model = await BuildViewModelAsync(larsCode, ukprn, submitModel, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        var validationResult = await setDateValidator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.Clear();
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, model);
        }

        submitModel.TryGetEnteredDate(out var lastDateStarts);

        await outerApiClient.UpsertProviderAllowedCourse(
            ukprn,
            larsCode,
            restrictedCourseProviderService.CreateUpsertRequest(User, lastDateStarts), cancellationToken);

        TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey] = model.IsChangingExistingDate
            ? $"{model.ProviderName} last start date has been updated"
            : $"Last start date added for {model.ProviderName}";

        return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
    }

    private async Task<SetLastDateStartsViewModel?> BuildViewModelAsync(
        string larsCode,
        int ukprn,
        SetLastDateStartsSubmitModel? submitModel,
        CancellationToken cancellationToken)
    {
        var courseAndProvider = await restrictedCourseProviderService.GetCourseAndProviderAsync(
            larsCode, ukprn, cancellationToken);
        if (courseAndProvider is null)
        {
            return null;
        }

        var (courseDetails, provider) = courseAndProvider.Value;
        var day = submitModel?.Day;
        var month = submitModel?.Month;
        var year = submitModel?.Year;

        if (submitModel is null && provider.LastDateStarts.HasValue)
        {
            var lastDateStarts = provider.LastDateStarts.Value;
            day = lastDateStarts.Day.ToString("00");
            month = lastDateStarts.Month.ToString("00");
            year = lastDateStarts.Year.ToString();
        }

        return new SetLastDateStartsViewModel
        {
            LarsCode = larsCode,
            Ukprn = ukprn,
            ProviderName = provider.ProviderName,
            CourseDisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(courseDetails.CourseName, courseDetails.Level),
            Day = day,
            Month = month,
            Year = year,
            CourseLastDateStarts = courseDetails.LastDateStarts,
            IsChangingExistingDate = provider.LastDateStarts.HasValue,
            CancelUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode })!
        };
    }
}

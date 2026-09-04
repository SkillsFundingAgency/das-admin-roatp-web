using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}/providers/{ukprn}/set-last-start-date", Name = RouteNames.SetLastDateStarts)]
public class SetLastDateStartsController(
    IOuterApiClient outerApiClient,
    IValidator<SetLastDateStartsSubmitModel> setLastDateStartsValidator) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/SetLastDateStarts/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        [FromRoute] int ukprn,
        CancellationToken cancellationToken)
    {
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
        submitModel.LarsCode = larsCode;
        var model = await BuildViewModelAsync(larsCode, ukprn, submitModel, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        submitModel.CourseLastDateStarts = model.CourseLastDateStarts;
        var validationResult = await setLastDateStartsValidator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, model);
        }

        var isValidDate = submitModel.TryGetEnteredDate(out var lastDateStarts);
        if (!isValidDate)
        {
            ModelState.AddModelError(string.Empty, SetLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage);
            return View(ViewPath, model);
        }

        var response = await outerApiClient.PatchProviderAllowedCourse(
            ukprn,
            larsCode,
            User.UserId(),
            User.UserDisplayName(),
            new PatchProviderAllowedCourseRequest { LastDateStarts = lastDateStarts },
            cancellationToken);

        if (response.IsNotFound())
        {
            return NotFound();
        }

        await response.EnsureSuccessStatusCodeAsync();

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
        var courseDetails = await GetCourseDetailsAsync(larsCode, cancellationToken);
        var provider = courseDetails?.Providers.FirstOrDefault(p => p.Ukprn == ukprn);
        if (courseDetails is null || provider is null)
        {
            return null;
        }

        var day = submitModel?.Day;
        var month = submitModel?.Month;
        var year = submitModel?.Year;

        if (submitModel is null && provider.LastDateStarts.HasValue)
        {
            var existingLastDateStarts = provider.LastDateStarts.Value;
            day = existingLastDateStarts.Day.ToString("00");
            month = existingLastDateStarts.Month.ToString("00");
            year = existingLastDateStarts.Year.ToString();
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

    private async Task<GetRestrictedCourseDetailsResponse?> GetCourseDetailsAsync(
        string larsCode,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetAllowedProvidersForCourse(larsCode, cancellationToken);
        if (response.IsNotFound())
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync();
        return response.Content;
    }
}

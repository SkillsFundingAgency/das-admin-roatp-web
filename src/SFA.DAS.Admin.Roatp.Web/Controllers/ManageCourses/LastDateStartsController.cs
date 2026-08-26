using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}/providers/{ukprn}")]
public class LastDateStartsController(
    IValidator<IUkprnAndLarsCodeValidator> ukprnAndLarsCodeValidator,
    ILarsCodeService larsCodeService,
    IOuterApiClient outerApiClient,
    IValidator<AddLastDateStartsSubmitModel> setDateValidator,
    IValidator<ChangeLastDateStartsSubmitModel> changeOptionValidator) : Controller
{
    public const string AddLastDateStartsViewPath = "~/Views/ManageCourses/AddLastDateStarts/Index.cshtml";
    public const string ChangeLastDateStartsViewPath = "~/Views/ManageCourses/ChangeLastDateStarts/Index.cshtml";
    public const string ChangingExistingLastDateStartsTempDataKey = "ChangingExistingLastDateStarts";

    [HttpGet("set-last-start-date", Name = RouteNames.SetLastDateStarts)]
    public async Task<IActionResult> SetLastDateStarts(
        [FromRoute] string larsCode,
        [FromRoute] int ukprn,
        CancellationToken cancellationToken)
    {
        if (!await IsRouteValidAsync(larsCode, ukprn, cancellationToken))
        {
            return NotFound();
        }

        var model = await BuildSetViewModelAsync(larsCode, ukprn, null, cancellationToken);
        return model is null ? NotFound() : View(AddLastDateStartsViewPath, model);
    }

    [HttpPost("set-last-start-date")]
    public async Task<IActionResult> SetLastDateStarts(
        [FromRoute] string larsCode,
        [FromRoute] int ukprn,
        AddLastDateStartsSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        if (!await IsRouteValidAsync(larsCode, ukprn, cancellationToken))
        {
            return NotFound();
        }

        submitModel.LarsCode = larsCode;
        var model = await BuildSetViewModelAsync(larsCode, ukprn, submitModel, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        var validationResult = await setDateValidator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.Clear();
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(AddLastDateStartsViewPath, model);
        }

        submitModel.TryGetEnteredDate(out var lastDateStarts);

        await outerApiClient.UpsertProviderAllowedCourse(
            ukprn,
            larsCode,
            new UpsertProviderAllowedCourseRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName(),
                LastDateStarts = lastDateStarts
            },
            cancellationToken);

        TempData.Remove(ChangingExistingLastDateStartsTempDataKey);
        TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey] = model.IsChangingExistingDate
            ? $"{model.ProviderName} last start date has been updated"
            : $"Last start date added for {model.ProviderName}";

        return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
    }

    [HttpGet("change-last-start-date", Name = RouteNames.ChangeLastDateStarts)]
    public async Task<IActionResult> ChangeLastDateStarts(
        [FromRoute] string larsCode,
        [FromRoute] int ukprn,
        CancellationToken cancellationToken)
    {
        if (!await IsRouteValidAsync(larsCode, ukprn, cancellationToken))
        {
            return NotFound();
        }

        var model = await BuildChangeViewModelAsync(larsCode, ukprn, null, cancellationToken);
        return model is null ? NotFound() : View(ChangeLastDateStartsViewPath, model);
    }

    [HttpPost("change-last-start-date")]
    public async Task<IActionResult> ChangeLastDateStarts(
        [FromRoute] string larsCode,
        [FromRoute] int ukprn,
        ChangeLastDateStartsSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        if (!await IsRouteValidAsync(larsCode, ukprn, cancellationToken))
        {
            return NotFound();
        }

        var model = await BuildChangeViewModelAsync(larsCode, ukprn, submitModel, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        var validationResult = await changeOptionValidator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.Clear();
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ChangeLastDateStartsViewPath, model);
        }

        if (submitModel.SelectedOption == ChangeLastDateStartsOptions.Change)
        {
            TempData[ChangingExistingLastDateStartsTempDataKey] = true;
            return RedirectToRoute(RouteNames.SetLastDateStarts, new { larsCode, ukprn });
        }

        await outerApiClient.UpsertProviderAllowedCourse(
            ukprn,
            larsCode,
            new UpsertProviderAllowedCourseRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName(),
                LastDateStarts = null
            },
            cancellationToken);

        TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey] =
            $"{model.ProviderName} last start date has been removed";

        return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
    }

    private async Task<bool> IsRouteValidAsync(string larsCode, int ukprn, CancellationToken cancellationToken)
    {
        var validationResult = await ukprnAndLarsCodeValidator.ValidateAsync(
            new UkprnAndLarsCodeModel { Ukprn = ukprn, LarsCode = larsCode },
            cancellationToken);

        return validationResult.IsValid;
    }

    private async Task<(GetRestrictedCourseDetailsResponse Course, ProviderCourseModel Provider)?> GetCourseAndProviderAsync(
        string larsCode,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var courseDetails = await larsCodeService.GetCourseDetailsAsync(larsCode, cancellationToken);
        if (courseDetails is null)
        {
            return null;
        }

        var provider = courseDetails.Providers.FirstOrDefault(p => p.Ukprn == ukprn);
        if (provider is null)
        {
            return null;
        }

        return (courseDetails, provider);
    }

    private bool CanAccessSetLastDateStarts(bool providerHasLastDateStarts)
    {
        if (!providerHasLastDateStarts)
        {
            return true;
        }

        if (TempData.Peek(ChangingExistingLastDateStartsTempDataKey) is true)
        {
            TempData.Keep(ChangingExistingLastDateStartsTempDataKey);
            return true;
        }

        return false;
    }

    private async Task<AddLastDateStartsViewModel?> BuildSetViewModelAsync(
        string larsCode,
        int ukprn,
        AddLastDateStartsSubmitModel? submitModel,
        CancellationToken cancellationToken)
    {
        var courseAndProvider = await GetCourseAndProviderAsync(larsCode, ukprn, cancellationToken);
        if (courseAndProvider is null)
        {
            return null;
        }

        var (courseDetails, provider) = courseAndProvider.Value;
        var isChangingExistingDate = provider.LastDateStarts.HasValue;
        if (!CanAccessSetLastDateStarts(isChangingExistingDate))
        {
            return null;
        }

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

        return new AddLastDateStartsViewModel
        {
            LarsCode = larsCode,
            Ukprn = ukprn,
            ProviderName = provider.ProviderName,
            CourseDisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(courseDetails.CourseName, courseDetails.Level),
            Day = day,
            Month = month,
            Year = year,
            CourseLastDateStarts = courseDetails.LastDateStarts,
            IsChangingExistingDate = isChangingExistingDate,
            CancelUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode })!
        };
    }

    private async Task<ChangeLastDateStartsViewModel?> BuildChangeViewModelAsync(
        string larsCode,
        int ukprn,
        ChangeLastDateStartsSubmitModel? submitModel,
        CancellationToken cancellationToken)
    {
        var courseAndProvider = await GetCourseAndProviderAsync(larsCode, ukprn, cancellationToken);
        if (courseAndProvider is null)
        {
            return null;
        }

        var (courseDetails, provider) = courseAndProvider.Value;
        if (provider.LastDateStarts is null)
        {
            return null;
        }

        return new ChangeLastDateStartsViewModel
        {
            LarsCode = larsCode,
            Ukprn = ukprn,
            ProviderName = provider.ProviderName,
            CourseDisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(courseDetails.CourseName, courseDetails.Level),
            LastDateStarts = provider.LastDateStarts.Value,
            SelectedOption = submitModel?.SelectedOption,
            CancelUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode })!
        };
    }
}

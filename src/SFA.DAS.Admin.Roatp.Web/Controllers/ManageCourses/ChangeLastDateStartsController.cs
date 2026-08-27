using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}/providers/{ukprn}/change-last-start-date", Name = RouteNames.ChangeLastDateStarts)]
public class ChangeLastDateStartsController(
    IValidator<IUkprnAndLarsCodeValidator> ukprnAndLarsCodeValidator,
    ILarsCodeService larsCodeService,
    IOuterApiClient outerApiClient,
    IValidator<ChangeLastDateStartsSubmitModel> changeOptionValidator) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/ChangeLastDateStarts/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        [FromRoute] int ukprn,
        CancellationToken cancellationToken)
    {
        if (!await IsRouteValidAsync(larsCode, ukprn, cancellationToken))
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
        ChangeLastDateStartsSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        if (!await IsRouteValidAsync(larsCode, ukprn, cancellationToken))
        {
            return NotFound();
        }

        var model = await BuildViewModelAsync(larsCode, ukprn, submitModel, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        var validationResult = await changeOptionValidator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.Clear();
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, model);
        }

        if (submitModel.SelectedOption == ChangeLastDateStartsOptions.Change)
        {
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

    private async Task<ChangeLastDateStartsViewModel?> BuildViewModelAsync(
        string larsCode,
        int ukprn,
        ChangeLastDateStartsSubmitModel? submitModel,
        CancellationToken cancellationToken)
    {
        var courseDetails = await larsCodeService.GetCourseDetailsAsync(larsCode, cancellationToken);
        var provider = courseDetails?.Providers.FirstOrDefault(p => p.Ukprn == ukprn);
        if (courseDetails is null || provider?.LastDateStarts is null)
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

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}/providers/{ukprn}/change-restriction", Name = RouteNames.ChangeCourseRestriction)]
public class ChangeCourseRestrictionController(
    IRestrictedCourseProviderService restrictedCourseProviderService,
    IOuterApiClient outerApiClient,
    IValidator<ChangeCourseRestrictionSubmitModel> changeOptionValidator) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/ChangeCourseRestriction/Index.cshtml";

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
        ChangeCourseRestrictionSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        if (!await restrictedCourseProviderService.IsRouteValidAsync(larsCode, ukprn, cancellationToken))
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

        if (submitModel.SelectedOption == ChangeCourseRestrictionOptions.Change)
        {
            return RedirectToRoute(RouteNames.SetLastDateStarts, new { larsCode, ukprn });
        }

        await outerApiClient.UpsertProviderAllowedCourse(
            ukprn,
            larsCode,
            restrictedCourseProviderService.CreateUpsertRequest(User, lastDateStarts: null), cancellationToken);

        TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey] =
            $"{model.ProviderName} last start date has been removed";

        return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
    }

    private async Task<ChangeCourseRestrictionViewModel?> BuildViewModelAsync(
        string larsCode,
        int ukprn,
        ChangeCourseRestrictionSubmitModel? submitModel,
        CancellationToken cancellationToken)
    {
        var courseAndProvider = await restrictedCourseProviderService.GetCourseAndProviderAsync(
            larsCode, ukprn, cancellationToken);
        if (courseAndProvider is null || courseAndProvider.Value.Provider.LastDateStarts is null)
        {
            return null;
        }

        var (courseDetails, provider) = courseAndProvider.Value;

        return new ChangeCourseRestrictionViewModel
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

using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using IApiResponse = Refit.IApiResponse;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}/providers/{ukprn}/change-restriction", Name = RouteNames.ChangeCourseRestriction)]
public class ChangeCourseRestrictionController(
    ILarsCodeService larsCodeService,
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

        var response = await outerApiClient.UpsertProviderAllowedCourse(
            ukprn,
            larsCode,
            CreateUpsertRequest(lastDateStarts: null),
            cancellationToken);

        if (IsNotFoundResponse(response))
        {
            return NotFound();
        }

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
        var courseDetails = await larsCodeService.GetCourseDetailsAsync(larsCode, cancellationToken);
        var provider = courseDetails?.Providers.FirstOrDefault(p => p.Ukprn == ukprn);
        if (courseDetails is null || provider is null || provider.LastDateStarts is null)
        {
            return null;
        }

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

    private UpsertProviderAllowedCourseRequest CreateUpsertRequest(DateTime? lastDateStarts)
        => new()
        {
            UserId = User.UserId(),
            UserDisplayName = User.UserDisplayName(),
            LastDateStarts = lastDateStarts
        };

    private static bool IsNotFoundResponse(IApiResponse response)
        => response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest;
}

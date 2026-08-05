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
[Route("restricted-courses/{larsCode}/providers/{ukprn}/add-last-date-starts", Name = RouteNames.AddLastDateStarts)]
public class AddLastDateStartsController(
    LarsCodeValidator larsCodeValidator,
    UkprnValidator ukprnValidator,
    ILarsCodeService larsCodeService,
    IOuterApiClient outerApiClient,
    IValidator<AddLastDateStartsSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/AddLastDateStarts/Index.cshtml";
    public const string SuccessBannerTempDataKey = "LastDateStartsSuccessMessage";

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
        AddLastDateStartsSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var model = await BuildViewModelAsync(larsCode, ukprn, submitModel, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        submitModel.CourseLastDateStarts = model.CourseLastDateStarts;

        var validationResult = await validator.ValidateAsync(submitModel, cancellationToken);
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
            new UpsertProviderAllowedCourseRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName(),
                LastDateStarts = lastDateStarts
            },
            cancellationToken);

        TempData[SuccessBannerTempDataKey] = $"Last start date added for {model.ProviderName}";

        return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
    }

    private async Task<AddLastDateStartsViewModel?> BuildViewModelAsync(
        string larsCode,
        int ukprn,
        AddLastDateStartsSubmitModel? submitModel,
        CancellationToken cancellationToken)
    {
        var larsCodeValidationResult = await larsCodeValidator.ValidateAsync(
            new LarsCodeModel { LarsCode = larsCode },
            cancellationToken);

        if (!larsCodeValidationResult.IsValid)
        {
            return null;
        }

        var ukprnValidationResult = await ukprnValidator.ValidateAsync(
            new UkprnModel { Ukprn = ukprn },
            cancellationToken);

        if (!ukprnValidationResult.IsValid)
        {
            return null;
        }

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

        RestrictedCourseDetailsViewModel courseDisplay = courseDetails;

        return new AddLastDateStartsViewModel
        {
            LarsCode = larsCode,
            Ukprn = ukprn,
            ProviderName = provider.ProviderName,
            CourseDisplayTitle = courseDisplay.DisplayTitle,
            Day = submitModel?.Day,
            Month = submitModel?.Month,
            Year = submitModel?.Year,
            CourseLastDateStarts = courseDetails.LastDateStarts,
            CancelUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode })!
        };
    }
}

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}/providers")]
public class AddProviderToRestrictedCourseController(
    LarsCodeValidator larsCodeValidator,
    ILarsCodeService larsCodeService,
    ISessionService sessionService,
    IValidator<AddProviderToRestrictedCourseSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/AddProviderToRestrictedCourse/Index.cshtml";

    [HttpGet("add", Name = RouteNames.AddProviderToRestrictedCourse)]
    public async Task<IActionResult> Index([FromRoute] string larsCode, CancellationToken cancellationToken)
    {
        sessionService.Delete(SessionKeys.AddProviderToRestrictedCourse);
        sessionService.Delete(SessionKeys.NotAllowedProvidersForRestrictedCourse(larsCode));

        var courseDetails = await GetRestrictedCourseAsync(larsCode, cancellationToken);
        if (courseDetails is null)
        {
            return NotFound();
        }

        return View(ViewPath, BuildViewModel(larsCode, courseDetails.CourseName, courseDetails.Level));
    }

    [HttpPost("add")]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        AddProviderToRestrictedCourseSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var courseDetails = await GetRestrictedCourseAsync(larsCode, cancellationToken);
        if (courseDetails is null)
        {
            return NotFound();
        }

        var validationResult = validator.Validate(submitModel);
        if (!validationResult.IsValid)
        {
            ModelState.Clear();
            ModelState.AddValidationErrors(validationResult.Errors);
            var viewModel = BuildViewModel(larsCode, courseDetails.CourseName, courseDetails.Level);
            viewModel.LegalName = submitModel.LegalName;
            viewModel.Ukprn = submitModel.Ukprn;
            return View(ViewPath, viewModel);
        }

        if (!int.TryParse(submitModel.Ukprn, out _))
        {
            ModelState.Clear();
            ModelState.AddModelError(
                nameof(AddProviderToRestrictedCourseSubmitModel.SearchTerm),
                AddProviderToRestrictedCourseSubmitModelValidator.NoProviderSelectedErrorMessage);
            return View(ViewPath, BuildViewModel(larsCode, courseDetails.CourseName, courseDetails.Level));
        }

        return RedirectToRoute(RouteNames.AddProviderToRestrictedCourse, new { larsCode });
    }

    private async Task<GetRestrictedCourseDetailsResponse?> GetRestrictedCourseAsync(
        string larsCode,
        CancellationToken cancellationToken)
    {
        var validationResult = await larsCodeValidator.ValidateAsync(
            new LarsCodeModel { LarsCode = larsCode },
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return null;
        }

        var courseDetails = await larsCodeService.GetCourseDetailsAsync(larsCode, cancellationToken);
        if (courseDetails is null || !courseDetails.IsCourseRestricted)
        {
            return null;
        }

        return courseDetails;
    }

    private AddProviderToRestrictedCourseViewModel BuildViewModel(string larsCode, string courseName, int level)
    {
        var viewModel = CreateCourseDisplay(larsCode, courseName, level);
        viewModel.ProvidersSearchUrl = Url.RouteUrl(RouteNames.SearchProvidersForRestrictedCourse, new { larsCode })!;
        return viewModel;
    }

    private static AddProviderToRestrictedCourseViewModel CreateCourseDisplay(string larsCode, string courseName, int level)
        => new()
        {
            LarsCode = larsCode,
            Title = courseName,
            Level = level
        };
}

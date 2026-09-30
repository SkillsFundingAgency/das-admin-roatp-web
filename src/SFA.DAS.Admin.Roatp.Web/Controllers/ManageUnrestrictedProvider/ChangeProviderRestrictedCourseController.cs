using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses/{larsCode}/change-restriction", Name = RouteNames.ChangeProviderRestrictedCourse)]
public class ChangeProviderRestrictedCourseController(
    IApplicationCacheService applicationCacheService,
    IValidator<ChangeProviderRestrictedCourseSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/ChangeProviderRestrictedCourse/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, string larsCode, CancellationToken cancellationToken = default)
    {
        var model = await BuildViewModel(ukprn, larsCode, cancellationToken: cancellationToken);
        return model is null
            ? RedirectToRoute(RouteNames.ProviderRestrictedCourses, new { ukprn })
            : View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        int ukprn,
        string larsCode,
        ChangeProviderRestrictedCourseSubmitModel submitModel,
        CancellationToken cancellationToken = default)
    {
        var model = await BuildViewModel(ukprn, larsCode, cancellationToken, submitModel);
        if (model is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedCourses, new { ukprn });
        }

        var validationResult = validator.Validate(submitModel);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
        }

        return View(ViewPath, model);
    }

    private async Task<ChangeProviderRestrictedCourseViewModel?> BuildViewModel(
        int ukprn,
        string larsCode,
        CancellationToken cancellationToken,
        ChangeProviderRestrictedCourseSubmitModel? submitModel = null)
    {
        var cachedRestrictedApprenticeship = await GetApplicationCachedRestrictedApprenticeship(ukprn, larsCode, cancellationToken);
        if (cachedRestrictedApprenticeship is null)
        {
            return null;
        }

        return new ChangeProviderRestrictedCourseViewModel
        {
            Ukprn = ukprn,
            LarsCode = cachedRestrictedApprenticeship.LarsCode,
            CourseDisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(cachedRestrictedApprenticeship.Title, cachedRestrictedApprenticeship.Level),
            LastDateStarts = cachedRestrictedApprenticeship.LastDateStarts,
            SelectedOption = submitModel?.SelectedOption,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderRestrictedCourses, new { ukprn })!
        };
    }

    private async Task<RestrictedApprenticeshipModel?> GetApplicationCachedRestrictedApprenticeship(
        int ukprn,
        string larsCode,
        CancellationToken cancellationToken)
    {
        var courses = await applicationCacheService.GetAsync<List<RestrictedApprenticeshipModel>>(
            ApplicationCacheKeys.RestrictedApprenticeships(ukprn),
            cancellationToken);

        return courses?.FirstOrDefault(course => course.LarsCode == larsCode);
    }
}

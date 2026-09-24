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
    public IActionResult Index(int ukprn, string larsCode)
    {
        var model = BuildViewModel(ukprn, larsCode);
        return model is null
            ? RedirectToRoute(RouteNames.ProviderRestrictedCourses, new { ukprn })
            : View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Index(int ukprn, string larsCode, ChangeProviderRestrictedCourseSubmitModel submitModel)
    {
        var model = BuildViewModel(ukprn, larsCode, submitModel);
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

    private ChangeProviderRestrictedCourseViewModel? BuildViewModel(
        int ukprn,
        string larsCode,
        ChangeProviderRestrictedCourseSubmitModel? submitModel = null)
    {
        var course = GetCachedCourse(ukprn, larsCode);
        if (course is null)
        {
            return null;
        }

        return new ChangeProviderRestrictedCourseViewModel
        {
            Ukprn = ukprn,
            LarsCode = course.LarsCode,
            DisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
            LastDateStarts = course.LastDateStarts,
            SelectedOption = submitModel?.SelectedOption,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderRestrictedCourses, new { ukprn })!
        };
    }

    private RestrictedApprenticeshipModel? GetCachedCourse(int ukprn, string larsCode)
    {
        if (!applicationCacheService.TryGet<List<RestrictedApprenticeshipModel>>(
                ApplicationCacheKeys.RestrictedApprenticeships(ukprn),
                out var courses)
            || courses is null)
        {
            return null;
        }

        return courses.FirstOrDefault(course => course.LarsCode == larsCode);
    }
}

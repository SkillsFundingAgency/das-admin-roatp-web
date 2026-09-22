using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses/add", Name = RouteNames.RestrictCourseSearch)]
public class RestrictCourseSearchController(
    IOuterApiClient outerApiClient,
    ISessionService sessionService,
    IValidator<RestrictCourseSearchSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/RestrictCourseSearch/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        sessionService.Delete(SessionKeys.RestrictCourse);

        var courses = await GetSearchableCoursesAsync(ukprn, cancellationToken);
        if (courses is null)
        {
            return NotFound();
        }

        return View(ViewPath, BuildViewModel(ukprn, courses));
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        int ukprn,
        RestrictCourseSearchSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(submitModel);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            var courses = await GetSearchableCoursesAsync(ukprn, cancellationToken) ?? [];
            return View(ViewPath, BuildViewModel(ukprn, courses));
        }

        var searchableCourses = await GetSearchableCoursesAsync(ukprn, cancellationToken);
        if (searchableCourses is null)
        {
            return NotFound();
        }

        var course = searchableCourses.FirstOrDefault(c => c.LarsCode == submitModel.SelectedLarsCode);
        if (course is null)
        {
            return NotFound();
        }

        sessionService.Set(SessionKeys.RestrictCourse, new RestrictCourseSessionModel
        {
            Ukprn = ukprn,
            LarsCode = course.LarsCode,
            Title = course.Title,
            Level = course.Level,
            DisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level)
        });

        return View(ViewPath, BuildViewModel(ukprn, searchableCourses, submitModel.SelectedLarsCode));
    }

    private async Task<List<RestrictedCourseModel>?> GetSearchableCoursesAsync(
        int ukprn,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetNotRestrictedApprenticeships(ukprn, cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return response.Content?.Courses ?? [];
    }

    private static RestrictCourseSearchViewModel BuildViewModel(
        int ukprn,
        IEnumerable<RestrictedCourseModel> courses,
        string? selectedLarsCode = null)
        => new()
        {
            Ukprn = ukprn,
            SelectedLarsCode = selectedLarsCode,
            Courses = courses
                .OrderBy(
                    course => CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
                    StringComparer.OrdinalIgnoreCase)
                .Select(course => new SelectListItem(
                    CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
                    course.LarsCode,
                    course.LarsCode == selectedLarsCode))
        };
}

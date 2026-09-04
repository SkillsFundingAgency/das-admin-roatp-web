using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("unrestricted-courses/search", Name = RouteNames.UnrestrictedCourseSearch)]
public class UnrestrictedCourseSearchController(
    IOuterApiClient outerApiClient,
    IValidator<UnrestrictedCourseSearchSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/UnrestrictedCourseSearch/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var courses = await GetUnrestrictedCoursesAsync(cancellationToken);
        return View(ViewPath, BuildViewModel(courses));
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        UnrestrictedCourseSearchSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var courses = await GetUnrestrictedCoursesAsync(cancellationToken);

        var validationResult = validator.Validate(submitModel);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, BuildViewModel(courses));
        }

        var course = courses.FirstOrDefault(c => c.LarsCode == submitModel.SelectedLarsCode);
        return RedirectToRoute(RouteNames.UnrestrictedCourseDetails, new { larsCode = course.LarsCode });
    }

    private async Task<List<RestrictedCourseModel>> GetUnrestrictedCoursesAsync(CancellationToken cancellationToken)
    {
        GetRestrictedCoursesResponse response = await outerApiClient.GetRestrictedCourses(
            restricted: false,
            cancellationToken);

        return response.Courses;
    }

    private static UnrestrictedCourseSearchViewModel BuildViewModel(IEnumerable<RestrictedCourseModel> courses)
        => new()
        {
            Courses = courses
                .OrderBy(course => course.Title)
                .ThenBy(course => course.Level)
                .Select(course => new SelectListItem(
                    $"{course.Title} (Level {course.Level})",
                    course.LarsCode))
        };
}

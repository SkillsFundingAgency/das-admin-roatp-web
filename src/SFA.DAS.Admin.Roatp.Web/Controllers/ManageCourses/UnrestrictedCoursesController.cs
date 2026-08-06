using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("unrestricted-courses", Name = RouteNames.UnrestrictedCourses)]
public class UnrestrictedCoursesController(IUnrestrictedCoursesService unrestrictedCoursesService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string query, CancellationToken cancellationToken)
    {
        query ??= string.Empty;
        var searchTerm = query.Trim();
        if (searchTerm.Length < 1) return Ok(Array.Empty<UnrestrictedCourseSearchItem>());

        var courses = await unrestrictedCoursesService.GetUnrestrictedCourses(cancellationToken);

        var matchedCourses = SearchCourses(courses, searchTerm);

        return Ok(matchedCourses);
    }

    private static IEnumerable<UnrestrictedCourseSearchItem> SearchCourses(IEnumerable<RestrictedCourseModel> courses, string searchTerm)
    {
        return courses
            .Where(course => MatchesSearchTerm(course, searchTerm))
            .OrderBy(course => course.Title)
            .ThenBy(course => course.Level)
            .Select(course => (UnrestrictedCourseSearchItem)course)
            .Take(100);
    }

    private static bool MatchesSearchTerm(RestrictedCourseModel course, string searchTerm)
    {
        var displayTitle = $"{course.Title} (Level {course.Level})";

        return displayTitle.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}

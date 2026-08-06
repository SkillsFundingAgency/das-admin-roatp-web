using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        var matchedCourses = courses
            .Where(course =>
                course.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || course.LarsCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(course => course.Title)
            .Select(course => (UnrestrictedCourseSearchItem)course)
            .Take(100);

        return Ok(matchedCourses);
    }
}

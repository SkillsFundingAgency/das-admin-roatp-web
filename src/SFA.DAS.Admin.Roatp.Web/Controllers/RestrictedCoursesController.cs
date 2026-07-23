using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Application.RestrictedCourses.Queries.GetRestrictedCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses", Name = RouteNames.RestrictedCourses)]
public class RestrictedCoursesController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRestrictedCoursesQuery(), cancellationToken);

        var model = new RestrictedCoursesViewModel
        {
            TotalCount = result.TotalCount,
            Courses = result.Courses.Select(course => new RestrictedCourseItemViewModel
            {
                LarsCode = course.LarsCode,
                Title = course.Title,
                Level = course.Level,
                LearningType = course.LearningType
            }).ToList()
        };

        return View(model);
    }
}

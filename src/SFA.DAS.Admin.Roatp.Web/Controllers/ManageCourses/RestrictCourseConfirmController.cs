using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/add/{larsCode}/confirm", Name = RouteNames.RestrictCourseConfirm)]
public class RestrictCourseConfirmController : Controller
{
    [HttpGet]
    public IActionResult Index([FromRoute] string larsCode)
    {
        return RedirectToRoute(RouteNames.UnrestrictedCourseDetails, new { larsCode });
    }
}

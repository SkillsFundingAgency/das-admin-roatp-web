using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses", Name = RouteNames.RestrictedCourses)]
public class RestrictedCoursesController(IOuterApiClient outerApiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetRestrictedCourses(restricted: true, cancellationToken);

        RestrictedCoursesViewModel model = response;

        return View(model);
    }
}

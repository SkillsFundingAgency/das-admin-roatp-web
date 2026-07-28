using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}", Name = RouteNames.RestrictedCourseDetails)]
public class RestrictedCourseDetailsController(LarsCodeValidator larsCodeValidator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string larsCode, CancellationToken cancellationToken)
    {
        var courseDetails = await larsCodeValidator.GetCourseDetailsAsync(larsCode, cancellationToken);

        if (courseDetails is null)
        {
            return RedirectToRoute(RouteNames.RestrictedCourses);
        }

        RestrictedCourseDetailsViewModel model = courseDetails;
        model.BackLinkUrl = Url.RouteUrl(RouteNames.RestrictedCourses)!;
        model.PageUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode })!;

        foreach (var provider in model.AllowedProviders)
        {
            provider.ChangeUrl = model.PageUrl;
        }

        return View(model);
    }
}

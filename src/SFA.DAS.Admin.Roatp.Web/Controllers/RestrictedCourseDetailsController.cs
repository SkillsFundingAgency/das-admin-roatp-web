using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}", Name = RouteNames.RestrictedCourseDetails)]
public class RestrictedCourseDetailsController(
    LarsCodeValidator larsCodeValidator,
    ILarsCodeService larsCodeService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string larsCode, CancellationToken cancellationToken)
    {
        var validationResult = await larsCodeValidator.ValidateAsync(larsCode, cancellationToken);

        if (!validationResult.IsValid)
        {
            return NotFound();
        }

        var courseDetails = await larsCodeService.GetCourseDetailsAsync(larsCode, cancellationToken);

        RestrictedCourseDetailsViewModel model = courseDetails!;
        model.RestrictedCourseDetailsPageUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode })!;

        foreach (var provider in model.AllowedProviders)
        {
            provider.ChangeUrl = model.RestrictedCourseDetailsPageUrl;
        }

        return View(model);
    }
}

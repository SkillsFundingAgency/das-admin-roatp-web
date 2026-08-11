using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("unrestricted-courses/{larsCode}", Name = RouteNames.UnrestrictedCourseDetails)]
public class UnrestrictedCourseDetailsController(
    LarsCodeValidator larsCodeValidator,
    ILarsCodeService larsCodeService) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/UnrestrictedCourseDetails/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string larsCode, CancellationToken cancellationToken)
    {
        var validationResult = await larsCodeValidator.ValidateAsync(
            new LarsCodeModel { LarsCode = larsCode },
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return NotFound();
        }

        var courseDetails = await larsCodeService.GetCourseDetailsAsync(larsCode, cancellationToken);

        if (courseDetails!.IsCourseRestricted)
        {
            return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
        }

        UnrestrictedCourseDetailsViewModel model = courseDetails;
        model.RestrictCourseUrl = Url.RouteUrl(RouteNames.UnrestrictedCourseDetails, new { larsCode })!;

        return View(ViewPath, model);
    }
}

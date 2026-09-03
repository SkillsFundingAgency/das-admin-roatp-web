using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("unrestricted-courses/{larsCode}", Name = RouteNames.UnrestrictedCourseDetails)]
public class UnrestrictedCourseDetailsController(
    IOuterApiClient outerApiClient,
    ISessionService sessionService) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/UnrestrictedCourseDetails/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string larsCode, CancellationToken cancellationToken)
    {
        sessionService.Delete(SessionKeys.AddRestrictedCourse);

        var courseDetails = await GetCourseDetailsAsync(larsCode, cancellationToken);
        if (courseDetails is null)
        {
            return NotFound();
        }

        if (courseDetails.IsCourseRestricted)
        {
            return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
        }

        UnrestrictedCourseDetailsViewModel model = courseDetails;

        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Index([FromRoute] string larsCode, RestrictCourseSubmitModel submitModel)
    {
        sessionService.Set(SessionKeys.AddRestrictedCourse, new AddRestrictedCourseSessionModel
        {
            LarsCode = submitModel.LarsCode!,
            DisplayName = submitModel.DisplayName!
        });

        return RedirectToRoute(RouteNames.RestrictCourseConfirm, new { larsCode });
    }

    private async Task<GetRestrictedCourseDetailsResponse?> GetCourseDetailsAsync(
        string larsCode,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetAllowedProvidersForCourse(larsCode, cancellationToken);
        if (response.IsNotFound())
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync();
        return response.Content;
    }
}

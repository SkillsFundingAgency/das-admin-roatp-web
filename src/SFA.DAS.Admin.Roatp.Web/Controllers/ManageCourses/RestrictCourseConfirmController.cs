using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/add/{larsCode}/confirm", Name = RouteNames.RestrictCourseConfirm)]
public class RestrictCourseConfirmController(
    ISessionService sessionService,
    IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/RestrictCourseConfirm/Index.cshtml";
    public const string SuccessBannerMessage = "This course is now restricted";

    [HttpGet]
    public IActionResult Index([FromRoute] string larsCode)
    {
        var session = GetRestrictedCourseSessionModel(larsCode);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.UnrestrictedCourseDetails, new { larsCode });
        }

        return View(ViewPath, BuildViewModel(session));
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] string larsCode, CancellationToken cancellationToken)
    {
        var session = GetRestrictedCourseSessionModel(larsCode);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.UnrestrictedCourseDetails, new { larsCode });
        }

        await outerApiClient.AddRestrictedCourse(
            new AddRestrictedCourseRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName(),
                LarsCode = session.LarsCode
            },
            cancellationToken);

        sessionService.Delete(SessionKeys.AddRestrictedCourse);
        TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey] = SuccessBannerMessage;

        return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
    }

    private AddRestrictedCourseSessionModel? GetRestrictedCourseSessionModel(string larsCode)
    {
        var session = sessionService.Get<AddRestrictedCourseSessionModel>(SessionKeys.AddRestrictedCourse);
        if (session is null || !string.Equals(session.LarsCode, larsCode, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return session;
    }

    private RestrictCourseConfirmViewModel BuildViewModel(AddRestrictedCourseSessionModel session)
    {
        return new RestrictCourseConfirmViewModel
        {
            LarsCode = session.LarsCode,
            DisplayName = session.DisplayName,
            CancelUrl = Url.RouteUrl(RouteNames.UnrestrictedCourseDetails, new { larsCode = session.LarsCode })!
        };
    }
}

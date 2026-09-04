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
[Route("restricted-courses/{larsCode}/providers/confirm-add", Name = RouteNames.ConfirmAddProviderToRestrictedCourse)]
public class ConfirmAddProviderToRestrictedCourseController(
    ISessionService sessionService,
    IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/ConfirmAddProviderToRestrictedCourse/Index.cshtml";

    [HttpGet]
    public IActionResult Index([FromRoute] string larsCode)
    {
        var session = GetAddProviderSessionModel(larsCode);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
        }

        return View(ViewPath, BuildViewModel(session));
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        CancellationToken cancellationToken)
    {
        var session = GetAddProviderSessionModel(larsCode);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
        }

        var response = await outerApiClient.UpsertProviderAllowedCourse(
            session.Ukprn,
            session.LarsCode,
            new UpsertProviderAllowedCourseRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName(),
                LastDateStarts = null
            },
            cancellationToken);

        if (response.IsNotFound())
        {
            return NotFound();
        }

        await response.EnsureSuccessStatusCodeAsync();

        sessionService.Delete(SessionKeys.AddProviderToRestrictedCourse);

        TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey] =
            $"{session.LegalName.ToUpperInvariant()} is now allowed to deliver this training";

        return RedirectToRoute(RouteNames.RestrictedCourseDetails, new { larsCode });
    }

    private AddProviderToRestrictedCourseSessionModel? GetAddProviderSessionModel(string larsCode)
    {
        var session = sessionService.Get<AddProviderToRestrictedCourseSessionModel>(SessionKeys.AddProviderToRestrictedCourse);
        if (session is null || !string.Equals(session.LarsCode, larsCode, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return session;
    }

    private ConfirmAddProviderToRestrictedCourseViewModel BuildViewModel(AddProviderToRestrictedCourseSessionModel session)
    {
        return new ConfirmAddProviderToRestrictedCourseViewModel
        {
            LarsCode = session.LarsCode,
            ProviderName = session.LegalName.ToUpperInvariant(),
            Ukprn = session.Ukprn,
            CourseDisplayTitle = session.CourseDisplayTitle,
            CancelUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode = session.LarsCode })!
        };
    }
}


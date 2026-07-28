using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}", Name = RouteNames.RestrictedCourseDetails)]
public class RestrictedCourseDetailsController(IOuterApiClient outerApiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string larsCode, int? level, CancellationToken cancellationToken)
    {
        var apiResponse = await outerApiClient.GetAllowedProvidersForCourse(larsCode, cancellationToken);

        if (apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.Content is null)
        {
            return RedirectToRoute(RouteNames.RestrictedCourses);
        }

        var courseLevel = level ?? await GetCourseLevel(larsCode, cancellationToken);
        var model = RestrictedCourseDetailsViewModel.FromResponse(apiResponse.Content, courseLevel);
        model.BackLinkUrl = Url.RouteUrl(RouteNames.RestrictedCourses)!;
        model.PageUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode, level = courseLevel })!;

        foreach (var provider in model.Providers)
        {
            provider.ChangeUrl = model.PageUrl;
        }

        return View(model);
    }

    private async Task<int> GetCourseLevel(string larsCode, CancellationToken cancellationToken)
    {
        var restrictedCourses = await outerApiClient.GetRestrictedCourses(restricted: true, cancellationToken);
        return restrictedCourses.Courses.FirstOrDefault(course => course.LarsCode == larsCode)?.Level ?? 0;
    }
}

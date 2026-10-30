using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/{routeLearningType}/allowed-courses/{larsCode}/remove-last-date-starts",
    Name = RouteNames.RemoveAllowedCourseLastStartDate)]
public class RemoveAllowedCourseLastStartDateController(
    IOuterApiClient outerApiClient,
    IUkprnService ukprnService,
    ICoursesService coursesService) : Controller
{
    public const string ViewPath = "~/Views/ManageRestrictedProvider/RemoveAllowedCourseLastStartDate/Index.cshtml";

    public static string GetSuccessBannerMessage(string displayTitle) =>
        $"{displayTitle} last start date has been removed";

    [HttpGet]
    public async Task<IActionResult> Index(
        int ukprn,
        RouteLearningType routeLearningType,
        string larsCode,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(routeLearningType))
        {
            return NotFound();
        }

        var organisation = await ukprnService.GetOrganisationAsync(ukprn, cancellationToken);
        if (organisation is null)
        {
            return NotFound();
        }

        if (organisation.IsUnrestrictedProvider(routeLearningType))
        {
            return NotFound();
        }

        ProviderAllowedCourseModel? allowedCourse = await outerApiClient.GetAllowedCourse(ukprn, larsCode, cancellationToken);
        if (allowedCourse is null || !allowedCourse.LastDateStarts.HasValue)
        {
            return NotFound();
        }

        var course = await coursesService.GetCourse(larsCode, cancellationToken);
        if (course is null || course.CourseType != routeLearningType.ToCourseType())
        {
            return NotFound();
        }

        return View(ViewPath, BuildViewModel(ukprn, routeLearningType, course));
    }

    [HttpPost]
    public async Task<IActionResult> IndexPost(
        int ukprn,
        RouteLearningType routeLearningType,
        string larsCode,
        CancellationToken cancellationToken)
    {
        var course = await coursesService.GetCourse(larsCode, cancellationToken);
        if (course is null)
        {
            return NotFound();
        }

        var response = await outerApiClient.PutProviderAllowedCourse(
            ukprn,
            larsCode,
            new PutProviderAllowedCourseRequest(null, User.UserId(), User.UserDisplayName()),
            cancellationToken);

        await response.EnsureSuccessStatusCodeAsync();

        TempData[ProviderAllowedCoursesController.SuccessBannerTempDataKey] =
            GetSuccessBannerMessage(CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level));

        return RedirectToRoute(RouteNames.ProviderAllowedCourses, new { ukprn, routeLearningType });
    }

    private RemoveAllowedCourseLastStartDateViewModel BuildViewModel(
        int ukprn,
        RouteLearningType routeLearningType,
        GetCourseResponse course)
        => new()
        {
            Ukprn = ukprn,
            RouteLearningType = routeLearningType,
            CourseDisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
            LarsCode = course.LarsCode,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderAllowedCourses, new { ukprn, routeLearningType })!
        };
}

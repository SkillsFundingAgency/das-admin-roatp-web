using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/{routeLearningType}/allowed-courses", Name = RouteNames.ProviderAllowedCourses)]
public class ProviderAllowedCoursesController(
    IOuterApiClient outerApiClient,
    IUkprnService ukprnService) : Controller
{
    public const string ViewPath = "~/Views/ManageRestrictedProvider/ProviderAllowedCourses/Index.cshtml";
    public const string SuccessBannerTempDataKey = "SuccessBannerMessage";

    [HttpGet]
    public async Task<IActionResult> Index(
        int ukprn,
        RouteLearningType routeLearningType,
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

        var courseType = routeLearningType.ToCourseType();
        if (routeLearningType == RouteLearningType.Apprenticeship && !organisation.IsRestrictedForCourseType(courseType))
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        var coursesResponse = await GetAllowedCourses(ukprn, courseType, cancellationToken);
        if (coursesResponse is null)
        {
            return NotFound();
        }

        var pageUrl = Url.RouteUrl(RouteNames.ProviderAllowedCourses, new { ukprn, routeLearningType })!;

        var courses = (coursesResponse.AllowedCourses ?? [])
            .OrderBy(
                course => CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
                StringComparer.OrdinalIgnoreCase)
            .Select(course =>
            {
                AllowedCourseItemViewModel item = course;
                item.ChangeUrl = pageUrl;
                return item;
            })
            .ToList();

        var viewModel = new AllowedCoursesViewModel
        {
            RouteLearningType = routeLearningType,
            ProviderName = organisation.LegalName,
            BackLinkUrl = Url.RouteUrl(RouteNames.ProviderSummary, new { ukprn })!,
            AddUrl = pageUrl,
            TotalCount = courses.Count,
            Courses = courses,
            SuccessBannerMessage = TempData?[SuccessBannerTempDataKey] as string
        };

        return View(ViewPath, viewModel);
    }

    private async Task<GetAllowedCoursesResponse?> GetAllowedCourses(
        int ukprn,
        CourseType courseType,
        CancellationToken cancellationToken)
    {
        var apiResponse = await outerApiClient.GetAllowedCourses(ukprn, courseType, cancellationToken);
        if (apiResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await apiResponse.EnsureSuccessStatusCodeAsync();
        return apiResponse.Content ?? new GetAllowedCoursesResponse();
    }
}

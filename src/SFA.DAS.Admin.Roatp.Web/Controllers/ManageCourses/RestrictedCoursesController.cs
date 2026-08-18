using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses", Name = RouteNames.RestrictedCourses)]
public class RestrictedCoursesController(IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/RestrictedCourses/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(
        GetRestrictedCoursesRequest request,
        CancellationToken cancellationToken)
    {
        GetRestrictedCoursesResponse response = await outerApiClient.GetRestrictedCourses(restricted: true, cancellationToken);

        RestrictedCoursesViewModel model = response;
        model.HasActiveFilters = request.HasFilters;
        model.Filters = RestrictedCoursesFilterBuilder.CreateFiltersViewModel(request, Url);

        var filteredCourses = RestrictedCoursesFilterBuilder
            .ApplyFilters(model.Courses, request)
            .OrderBy(course => course.DisplayTitle, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ApplyPagination(model, filteredCourses, request);

        return View(ViewPath, model);
    }

    private void ApplyPagination(
        RestrictedCoursesViewModel model,
        List<RestrictedCourseItemViewModel> filteredCourses,
        GetRestrictedCoursesRequest request)
    {
        var pageSize = PaginationViewModel.DefaultPageSize;
        var totalCount = filteredCourses.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        var pageNumber = request.PageNumber < 1 ? 1 : Math.Min(request.PageNumber, totalPages);

        model.TotalCount = totalCount;
        model.Courses = filteredCourses
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        model.Pagination = new PaginationViewModel(
            pageNumber,
            model.TotalCount,
            pageSize,
            Url,
            RouteNames.RestrictedCourses,
            request.ToQueryString(),
            RestrictedCoursesFilterBuilder.RestrictedCourseFilterResultsFragment);
    }
}

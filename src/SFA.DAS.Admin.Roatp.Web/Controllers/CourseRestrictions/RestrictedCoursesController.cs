using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.CourseRestrictions;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses", Name = RouteNames.RestrictedCourses)]
public class RestrictedCoursesController(IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/CourseRestrictions/RestrictedCourses/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(
        GetRestrictedCoursesRequestModel requestModel,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetRestrictedCourses(restricted: true, cancellationToken);
        var courses = response?.Courses ?? [];

        var viewModel = new RestrictedCoursesViewModel
        {
            HasActiveFilters = requestModel.HasFilters,
            Filters = RestrictedCoursesFilterBuilder.CreateFiltersViewModel(requestModel, Url)
        };

        var filteredCourses = RestrictedCoursesFilterBuilder
            .ApplyFilters(courses, requestModel)
            .OrderBy(
                course => CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
                StringComparer.OrdinalIgnoreCase)
            .ToList();

        ApplyPagination(viewModel, filteredCourses, requestModel);

        return View(ViewPath, viewModel);
    }

    private void ApplyPagination(
        RestrictedCoursesViewModel viewModel,
        List<RestrictedCourseModel> filteredCourses,
        GetRestrictedCoursesRequestModel requestModel)
    {
        var (pagedItems, totalCount, pagination) = PaginationHelper.Paginate(
            filteredCourses,
            requestModel.PageNumber,
            Url,
            RouteNames.RestrictedCourses,
            requestModel.ToQueryString(),
            RestrictedCoursesFilterBuilder.RestrictedCourseFilterResultsFragment);

        viewModel.TotalCount = totalCount;
        viewModel.Courses = pagedItems
            .Select(course => (RestrictedCourseItemViewModel)course)
            .ToList();
        viewModel.Pagination = pagination;
    }
}

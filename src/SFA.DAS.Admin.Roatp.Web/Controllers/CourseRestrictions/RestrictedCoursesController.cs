using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
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
        GetRestrictedCoursesModel model,
        CancellationToken cancellationToken)
    {
        GetRestrictedCoursesResponse response = await outerApiClient.GetRestrictedCourses(restricted: true, cancellationToken);

        RestrictedCoursesViewModel viewModel = response;
        viewModel.HasActiveFilters = model.HasFilters;
        viewModel.Filters = RestrictedCoursesFilterBuilder.CreateFiltersViewModel(model, Url);

        var filteredCourses = RestrictedCoursesFilterBuilder
            .ApplyFilters(viewModel.Courses, model)
            .OrderBy(course => course.DisplayTitle, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ApplyPagination(viewModel, filteredCourses, model);

        return View(ViewPath, viewModel);
    }

    private void ApplyPagination(
        RestrictedCoursesViewModel viewModel,
        List<RestrictedCourseItemViewModel> filteredCourses,
        GetRestrictedCoursesModel model)
    {
        var (pagedItems, totalCount, pagination) = PaginationHelper.Paginate(
            filteredCourses,
            model.PageNumber,
            Url,
            RouteNames.RestrictedCourses,
            model.ToQueryString(),
            RestrictedCoursesFilterBuilder.RestrictedCourseFilterResultsFragment);

        viewModel.TotalCount = totalCount;
        viewModel.Courses = pagedItems;
        viewModel.Pagination = pagination;
    }
}

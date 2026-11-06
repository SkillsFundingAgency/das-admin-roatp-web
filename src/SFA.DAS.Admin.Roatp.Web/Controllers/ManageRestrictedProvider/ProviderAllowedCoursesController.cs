using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;
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
        GetProviderAllowedCoursesRequestModel requestModel,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(routeLearningType))
        {
            return NotFound();
        }

        GetOrganisationResponse? organisation = await ukprnService.GetOrganisationAsync(ukprn, cancellationToken);
        if (organisation is null)
        {
            return NotFound();
        }

        if (organisation.IsUnrestrictedProvider(routeLearningType))
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        var courseType = routeLearningType.ToCourseType();

        var coursesResponse = await GetAllowedCourses(ukprn, courseType, cancellationToken);
        if (coursesResponse is null)
        {
            return NotFound();
        }

        var filteredCourses = AllowedCoursesFilterBuilder
            .ApplyFilters(coursesResponse.AllowedCourses ?? [], requestModel)
            .OrderBy(
                course => CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
                StringComparer.OrdinalIgnoreCase)
            .ToList();

        var viewModel = new AllowedCoursesViewModel
        {
            RouteLearningType = routeLearningType,
            ProviderName = organisation.LegalName,
            BackLinkUrl = Url.RouteUrl(RouteNames.ProviderSummary, new { ukprn })!,
            AddUrl = Url.RouteUrl(RouteNames.AddAllowedCourse, new { ukprn, routeLearningType })!,
            HasActiveFilters = requestModel.HasFilters,
            Filters = AllowedCoursesFilterBuilder.CreateFiltersViewModel(
                requestModel,
                ukprn,
                routeLearningType,
                Url),
            SuccessBannerMessage = TempData?[SuccessBannerTempDataKey] as string
        };

        ApplyPagination(viewModel, filteredCourses, requestModel, ukprn, routeLearningType);

        return View(ViewPath, viewModel);
    }

    private void ApplyPagination(
        AllowedCoursesViewModel viewModel,
        List<ProviderAllowedCourseModel> filteredCourses,
        GetProviderAllowedCoursesRequestModel requestModel,
        int ukprn,
        RouteLearningType routeLearningType)
    {
        var (pagedItems, totalCount, pagination) = PaginationHelper.Paginate(
            filteredCourses,
            requestModel.PageNumber,
            Url,
            RouteNames.ProviderAllowedCourses,
            requestModel.ToQueryString(),
            AllowedCoursesFilterBuilder.AllowedCourseFilterResultsFragment);

        viewModel.TotalCount = totalCount;
        viewModel.Courses = pagedItems
            .Select(course =>
            {
                AllowedCourseItemViewModel item = course;
                item.ChangeUrl = course.LastDateStarts.HasValue
                    ? Url.RouteUrl(
                        RouteNames.ChangeAllowedCourseLastStartDate,
                        new { ukprn, routeLearningType, larsCode = course.LarsCode })!
                    : Url.RouteUrl(
                        RouteNames.AddAllowedCourseLastStartDate,
                        new { ukprn, routeLearningType, larsCode = course.LarsCode })!;
                return item;
            })
            .ToList();
        viewModel.Pagination = pagination;
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

using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses", Name = RouteNames.ProviderRestrictedApprenticeships)]
public class ProviderRestrictedApprenticeshipsController(
    IOuterApiClient outerApiClient,
    ISessionService sessionService,
    IUkprnService ukprnService) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/ProviderRestrictedApprenticeships/Index.cshtml";
    public const string SuccessBannerTempDataKey = "SuccessBannerMessage";

    [HttpGet]
    public async Task<IActionResult> Index(
        int ukprn,
        GetProviderRestrictedApprenticeshipsRequestModel requestModel,
        CancellationToken cancellationToken)
    {
        sessionService.Delete(SessionKeys.RestrictedCourseChangeRestriction);

        var providerName = await sessionService.GetProviderName(outerApiClient, ukprn, cancellationToken);
        if (providerName is null)
        {
            return NotFound();
        }

        var organisation = await ukprnService.GetOrganisationAsync(ukprn, cancellationToken);
        if (organisation is null)
        {
            return NotFound();
        }

        if (organisation.IsRestrictedForCourseType(CourseType.Apprenticeship))
        {
            return RedirectToRoute(RouteNames.ProviderAllowedCourses, new { ukprn, courseType = CourseType.Apprenticeship });
        }

        var coursesResponse = await GetRestrictedApprenticeships(ukprn, cancellationToken);
        if (coursesResponse is null)
        {
            return NotFound();
        }

        var courses = coursesResponse.Courses ?? [];

        var viewModel = new RestrictedApprenticeshipsViewModel
        {
            ProviderName = providerName,
            BackLinkUrl = Url.RouteUrl(RouteNames.ProviderSummary, new { ukprn })!,
            RestrictACourseUrl = Url.RouteUrl(RouteNames.ProviderRestrictedCourseSearch, new { ukprn })!,
            SuccessBannerMessage = TempData[SuccessBannerTempDataKey] as string,
            HasActiveFilters = requestModel.HasFilters,
            Filters = RestrictedApprenticeshipsFilterBuilder.CreateFiltersViewModel(requestModel, ukprn, Url)
        };

        var filteredCourses = RestrictedApprenticeshipsFilterBuilder
            .ApplyFilters(courses, requestModel)
            .OrderBy(
                course => CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
                StringComparer.OrdinalIgnoreCase)
            .ToList();

        ApplyPagination(viewModel, filteredCourses, requestModel, ukprn);

        return View(ViewPath, viewModel);
    }

    private async Task<GetRestrictedApprenticeshipsResponse?> GetRestrictedApprenticeships(
        int ukprn,
        CancellationToken cancellationToken)
    {
        var apiResponse = await outerApiClient.GetRestrictedApprenticeships(ukprn, cancellationToken);
        if (apiResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await apiResponse.EnsureSuccessfulAsync();
        return apiResponse.Content ?? new GetRestrictedApprenticeshipsResponse();
    }

    private void ApplyPagination(
        RestrictedApprenticeshipsViewModel viewModel,
        List<ProviderRestrictedApprenticeshipModel> filteredCourses,
        GetProviderRestrictedApprenticeshipsRequestModel requestModel,
        int ukprn)
    {
        var (pagedItems, totalCount, pagination) = PaginationHelper.Paginate(
            filteredCourses,
            requestModel.PageNumber,
            Url,
            RouteNames.ProviderRestrictedApprenticeships,
            requestModel.ToQueryString(),
            RestrictedApprenticeshipsFilterBuilder.RestrictedApprenticeshipFilterResultsFragment);

        viewModel.TotalCount = totalCount;
        viewModel.Courses = pagedItems
            .Select(course =>
            {
                RestrictedApprenticeshipItemViewModel item = course;
                item.ChangeUrl = Url.RouteUrl(
                    RouteNames.RestrictedCourseChangeRestriction,
                    new { ukprn, larsCode = course.LarsCode })!;
                return item;
            })
            .ToList();
        viewModel.Pagination = pagination;
    }
}

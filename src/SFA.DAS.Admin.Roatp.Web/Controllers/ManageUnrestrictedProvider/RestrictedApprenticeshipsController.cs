using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses", Name = RouteNames.ProviderRestrictedCourses)]
public class RestrictedApprenticeshipsController(IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/RestrictedApprenticeships/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(
        int ukprn,
        GetRestrictedApprenticeshipsModel model,
        CancellationToken cancellationToken)
    {
        var providerName = await GetProviderName(ukprn, cancellationToken);
        if (providerName is null)
        {
            return NotFound();
        }

        var apiResponse = await outerApiClient.GetRestrictedApprenticeships(ukprn, cancellationToken);
        if (apiResponse.StatusCode != HttpStatusCode.OK)
        {
            return NotFound();
        }

        var courses = apiResponse.Content?.Courses ?? [];
        var viewModel = new RestrictedApprenticeshipsViewModel
        {
            ProviderName = providerName,
            BackLinkUrl = Url.RouteUrl(RouteNames.ProviderSummary, new { ukprn })!,
            RestrictACourseUrl = Url.RouteUrl(RouteNames.ProviderRestrictedCourses, new { ukprn })!,
            HasActiveFilters = model.HasFilters,
            Filters = RestrictedApprenticeshipsFilterBuilder.CreateFiltersViewModel(model, ukprn, Url)
        };

        var filteredCourses = RestrictedApprenticeshipsFilterBuilder
            .ApplyFilters(courses, model)
            .OrderBy(
                course => CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
                StringComparer.OrdinalIgnoreCase)
            .ToList();

        ApplyPagination(viewModel, filteredCourses, model);

        return View(ViewPath, viewModel);
    }

    private void ApplyPagination(
        RestrictedApprenticeshipsViewModel viewModel,
        List<RestrictedApprenticeshipModel> filteredCourses,
        GetRestrictedApprenticeshipsModel model)
    {
        var (pagedItems, totalCount, pagination) = PaginationHelper.Paginate(
            filteredCourses,
            model.PageNumber,
            Url,
            RouteNames.ProviderRestrictedCourses,
            model.ToQueryString(),
            RestrictedApprenticeshipsFilterBuilder.RestrictedApprenticeshipFilterResultsFragment);

        viewModel.TotalCount = totalCount;
        viewModel.Courses = pagedItems
            .Select(course => (RestrictedApprenticeshipItemViewModel)course)
            .ToList();
        viewModel.Pagination = pagination;
    }

    private async Task<string?> GetProviderName(int ukprn, CancellationToken cancellationToken)
    {
        var cachedProviderName = TempData.Peek(TempDataKeys.ProviderLegalName) as string;
        if (!string.IsNullOrWhiteSpace(cachedProviderName))
        {
            TempData.Keep(TempDataKeys.ProviderLegalName);
            return cachedProviderName;
        }

        var organisationApiResponse = await outerApiClient.GetOrganisation(ukprn, cancellationToken);
        if (organisationApiResponse.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var providerName = organisationApiResponse.Content!.LegalName;
        TempData[TempDataKeys.ProviderLegalName] = providerName;
        return providerName;
    }
}

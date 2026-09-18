using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        RestrictedApprenticeshipsViewModel viewModel = apiResponse.Content!;
        viewModel.ProviderName = providerName;
        viewModel.BackLinkUrl = Url.RouteUrl(RouteNames.ProviderSummary, new { ukprn })!;
        viewModel.RestrictACourseUrl = Url.RouteUrl(RouteNames.ProviderRestrictedCourses, new { ukprn })!;
        viewModel.HasActiveFilters = model.HasFilters;
        viewModel.Filters = RestrictedApprenticeshipsFilterBuilder.CreateFiltersViewModel(model, ukprn, Url);

        var filteredCourses = RestrictedApprenticeshipsFilterBuilder
            .ApplyFilters(viewModel.Courses, model)
            .ToList();

        ApplyPagination(viewModel, filteredCourses, model);

        return View(ViewPath, viewModel);
    }

    private void ApplyPagination(
        RestrictedApprenticeshipsViewModel viewModel,
        List<RestrictedApprenticeshipItemViewModel> filteredCourses,
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
        viewModel.Courses = pagedItems;
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

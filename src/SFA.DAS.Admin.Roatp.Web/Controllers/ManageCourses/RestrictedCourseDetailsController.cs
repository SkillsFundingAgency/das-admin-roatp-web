using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}", Name = RouteNames.RestrictedCourseDetails)]
public class RestrictedCourseDetailsController(IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/RestrictedCourseDetails/Index.cshtml";
    public const string SuccessBannerTempDataKey = "SuccessBannerMessage";

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        GetRestrictedCourseDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var courseDetails = await GetCourseDetailsAsync(larsCode, cancellationToken);
        if (courseDetails is null)
        {
            return NotFound();
        }

        if (!courseDetails.IsCourseRestricted)
        {
            return RedirectToRoute(RouteNames.UnrestrictedCourseDetails, new { larsCode });
        }

        RestrictedCourseDetailsViewModel model = courseDetails;
        model.RestrictedCourseDetailsPageUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode })!;
        model.HasActiveFilters = request.HasFilters;
        model.Filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(request, larsCode, Url);

        var filteredProviders = RestrictedCourseDetailsFilterBuilder
            .ApplyFilters(model.AllowedProviders, request)
            .ToList();

        ApplyPagination(model, filteredProviders, request);

        foreach (var provider in model.AllowedProviders)
        {
            provider.ChangeUrl = provider.HasLastDateStarts
                ? Url.RouteUrl(RouteNames.ChangeCourseRestriction, new { larsCode, ukprn = provider.Ukprn })!
                : Url.RouteUrl(RouteNames.SetLastDateStarts, new { larsCode, ukprn = provider.Ukprn })!;
        }

        model.SuccessBannerMessage = TempData?[SuccessBannerTempDataKey] as string;

        return View(ViewPath, model);
    }

    private async Task<GetRestrictedCourseDetailsResponse?> GetCourseDetailsAsync(
        string larsCode,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetAllowedProvidersForCourse(larsCode, cancellationToken);
        if (response.IsNotFound())
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync();
        return response.Content;
    }

    private void ApplyPagination(
        RestrictedCourseDetailsViewModel model,
        List<AllowedProviderViewModel> filteredProviders,
        GetRestrictedCourseDetailsRequest request)
    {
        var (pagedItems, totalCount, pagination) = PaginationHelper.Paginate(
            filteredProviders,
            request.PageNumber,
            Url,
            RouteNames.RestrictedCourseDetails,
            request.ToQueryString(),
            RestrictedCourseDetailsFilterBuilder.ProviderFilterResultsFragment);

        model.TotalProviderCount = totalCount;
        model.AllowedProviders = pagedItems;
        model.Pagination = pagination;
    }
}

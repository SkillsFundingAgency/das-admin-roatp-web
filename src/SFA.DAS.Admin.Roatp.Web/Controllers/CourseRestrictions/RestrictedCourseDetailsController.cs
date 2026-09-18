using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.CourseRestrictions;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}", Name = RouteNames.RestrictedCourseDetails)]
public class RestrictedCourseDetailsController(IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/CourseRestrictions/RestrictedCourseDetails/Index.cshtml";
    public const string SuccessBannerTempDataKey = "SuccessBannerMessage";

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        GetRestrictedCourseDetailsModel model,
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

        RestrictedCourseDetailsViewModel viewModel = courseDetails;
        viewModel.RestrictedCourseDetailsPageUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode })!;
        viewModel.AddProviderUrl = Url.RouteUrl(RouteNames.AddProviderToRestrictedCourse, new { larsCode })!;
        viewModel.HasActiveFilters = model.HasFilters;
        viewModel.Filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(model, larsCode, Url);

        var filteredProviders = RestrictedCourseDetailsFilterBuilder
            .ApplyFilters(viewModel.AllowedProviders, model)
            .ToList();

        ApplyPagination(viewModel, filteredProviders, model);

        foreach (var provider in viewModel.AllowedProviders)
        {
            provider.ChangeUrl = provider.HasLastDateStarts
                ? Url.RouteUrl(RouteNames.ChangeCourseRestriction, new { larsCode, ukprn = provider.Ukprn })!
                : Url.RouteUrl(RouteNames.SetLastDateStarts, new { larsCode, ukprn = provider.Ukprn })!;
        }

        viewModel.SuccessBannerMessage = TempData?[SuccessBannerTempDataKey] as string;

        return View(ViewPath, viewModel);
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
        RestrictedCourseDetailsViewModel viewModel,
        List<AllowedProviderViewModel> filteredProviders,
        GetRestrictedCourseDetailsModel model)
    {
        var (pagedItems, totalCount, pagination) = PaginationHelper.Paginate(
            filteredProviders,
            model.PageNumber,
            Url,
            RouteNames.RestrictedCourseDetails,
            model.ToQueryString(),
            RestrictedCourseDetailsFilterBuilder.ProviderFilterResultsFragment);

        viewModel.TotalProviderCount = totalCount;
        viewModel.AllowedProviders = pagedItems;
        viewModel.Pagination = pagination;
    }
}

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
        GetRestrictedCourseDetailsRequestModel requestModel,
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

        var viewModel = new RestrictedCourseDetailsViewModel
        {
            LarsCode = courseDetails.LarsCode,
            CourseName = courseDetails.CourseName,
            Level = courseDetails.Level,
            Title = courseDetails.CourseName,
            Sector = courseDetails.Route,
            LearningType = courseDetails.LearningType,
            IsCourseRestricted = courseDetails.IsCourseRestricted,
            RestrictedCourseDetailsPageUrl = Url.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode })!,
            AddProviderUrl = Url.RouteUrl(RouteNames.AddProviderToRestrictedCourse, new { larsCode })!,
            HasActiveFilters = requestModel.HasFilters,
            Filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(requestModel, larsCode, Url),
            SuccessBannerMessage = TempData?[SuccessBannerTempDataKey] as string
        };

        var filteredProviders = RestrictedCourseDetailsFilterBuilder
            .ApplyFilters(courseDetails.Providers, requestModel)
            .OrderBy(provider => provider.ProviderName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ApplyPagination(viewModel, filteredProviders, requestModel);

        foreach (var provider in viewModel.AllowedProviders)
        {
            provider.ChangeUrl = provider.HasLastDateStarts
                ? Url.RouteUrl(RouteNames.ChangeCourseRestriction, new { larsCode, ukprn = provider.Ukprn })!
                : Url.RouteUrl(RouteNames.SetLastDateStarts, new { larsCode, ukprn = provider.Ukprn })!;
        }

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
        List<ProviderCourseModel> filteredProviders,
        GetRestrictedCourseDetailsRequestModel requestModel)
    {
        var (pagedItems, totalCount, pagination) = PaginationHelper.Paginate(
            filteredProviders,
            requestModel.PageNumber,
            Url,
            RouteNames.RestrictedCourseDetails,
            requestModel.ToQueryString(),
            RestrictedCourseDetailsFilterBuilder.ProviderFilterResultsFragment);

        viewModel.TotalProviderCount = totalCount;
        viewModel.AllowedProviders = pagedItems
            .Select(provider => (AllowedProviderViewModel)provider)
            .ToList();
        viewModel.Pagination = pagination;
    }
}

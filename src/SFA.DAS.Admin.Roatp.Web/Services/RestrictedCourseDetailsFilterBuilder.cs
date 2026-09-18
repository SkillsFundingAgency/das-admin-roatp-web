using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public static class RestrictedCourseDetailsFilterBuilder
{
    public const string ProviderFilterResultsFragment = "provider-results";

    private const string SearchTermInputId = "search-term-input";
    private const string DeliveryStatusFilterId = "delivery-status-filter";
    private const string OpenToNewStartsDescription = "Training providers offer this course on Find apprenticeship training.";
    private const string LastStartDateAddedDescription = "Training providers cannot accept new learners after this date.";
    private const string ClosedToNewStartsDescription = "Training providers are no longer allowed to offer this course.";

    public static FiltersViewModel CreateFiltersViewModel(
        GetRestrictedCourseDetailsRequestModel requestModel,
        string larsCode,
        IUrlHelper urlHelper)
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>();
        AddSelectedFilter(selectedFilters, FilterType.SearchTerm, requestModel.SearchTerm?.Trim());
        AddSelectedFilter(
            selectedFilters,
            FilterType.DeliveryStatus,
            requestModel.DeliveryStatus.Distinct().Select(status => status.ToString()));

        var clearFiltersBaseUrl = urlHelper.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode })!;

        return new FiltersViewModel
        {
            Route = RouteNames.RestrictedCourseDetails,
            LarsCode = larsCode,
            FilterResultsFragment = ProviderFilterResultsFragment,
            FilterSections =
            [
                CreateInputFilterSection(
                    SearchTermInputId,
                    SearchTermSectionHeading,
                    SearchTermSectionSubHeading,
                    nameof(FilterType.SearchTerm),
                    requestModel.SearchTerm),
                CreateCheckboxListFilterSection(
                    DeliveryStatusFilterId,
                    nameof(FilterType.DeliveryStatus),
                    DeliveryStatusSectionHeading,
                    null,
                    BuildDeliveryStatusItems(requestModel))
            ],
            ClearFilterSections = CreateClearFilterSections(
                selectedFilters,
                clearFiltersBaseUrl,
                useDisplayText: true,
                ProviderFilterResultsFragment)
        };
    }

    public static IEnumerable<ProviderCourseModel> ApplyFilters(
        IEnumerable<ProviderCourseModel> providers,
        GetRestrictedCourseDetailsRequestModel requestModel)
    {
        var filtered = providers;

        if (requestModel.HasSearchTermFilter)
        {
            var searchTerm = requestModel.SearchTerm.Trim();
            filtered = filtered.Where(provider =>
                provider.ProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || provider.Ukprn.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (requestModel.HasDeliveryStatusFilter)
        {
            var selectedStatuses = requestModel.DeliveryStatus.Distinct().ToHashSet();
            filtered = filtered.Where(provider =>
                selectedStatuses.Contains(provider.LastDateStarts.ToDeliveryStatus()));
        }

        return filtered;
    }

    private static List<FilterItemViewModel> BuildDeliveryStatusItems(GetRestrictedCourseDetailsRequestModel requestModel)
        =>
        [
            CreateDeliveryStatusItem(
                DeliveryStatus.OpenToNewStarts,
                requestModel,
                OpenToNewStartsDescription),
            CreateDeliveryStatusItem(
                DeliveryStatus.LastStartDateAdded,
                requestModel,
                LastStartDateAddedDescription),
            CreateDeliveryStatusItem(
                DeliveryStatus.ClosedToNewStarts,
                requestModel,
                ClosedToNewStartsDescription)
        ];

    private static FilterItemViewModel CreateDeliveryStatusItem(
        DeliveryStatus status,
        GetRestrictedCourseDetailsRequestModel requestModel,
        string description)
        => new()
        {
            Value = status.ToString(),
            DisplayText = status.GetDescription(),
            DisplayDescription = description,
            IsSelected = requestModel.DeliveryStatus.Contains(status)
        };
}

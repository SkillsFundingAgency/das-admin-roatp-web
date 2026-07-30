using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public static class RestrictedCourseDetailsFilterBuilder
{
    public static FiltersViewModel CreateFiltersViewModel(
        GetRestrictedCourseDetailsRequest request,
        string larsCode,
        IUrlHelper urlHelper)
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>();
        AddSelectedFilter(selectedFilters, FilterType.ProviderName, request.ProviderName?.Trim());
        AddSelectedFilter(
            selectedFilters,
            FilterType.DeliveryStatus,
            request.DeliveryStatus.Distinct().Select(status => status.GetDescription()));

        var overrideValueFunctions = new Dictionary<FilterType, Func<string, string>>
        {
            [FilterType.DeliveryStatus] = displayText =>
                Enum.GetValues<DeliveryStatus>()
                    .First(status => status.GetDescription() == displayText)
                    .ToString()
        };

        var clearFiltersBaseUrl = urlHelper.RouteUrl(RouteNames.RestrictedCourseDetails, new { larsCode })!;

        return new FiltersViewModel
        {
            Route = RouteNames.RestrictedCourseDetails,
            LarsCode = larsCode,
            FilterSections =
            [
                CreateInputFilterSection(
                    "provider-name-input",
                    ProviderNameSectionHeading,
                    ProviderNameSectionSubHeading,
                    nameof(FilterType.ProviderName),
                    request.ProviderName),
                CreateCheckboxListFilterSection(
                    "delivery-status-filter",
                    nameof(FilterType.DeliveryStatus),
                    DeliveryStatusSectionHeading,
                    null,
                    BuildDeliveryStatusItems(request))
            ],
            ClearFilterSections = CreateClearFilterSections(
                selectedFilters,
                clearFiltersBaseUrl,
                overrideValueFunctions)
        };
    }

    public static IEnumerable<AllowedProviderViewModel> ApplyFilters(
        IEnumerable<AllowedProviderViewModel> providers,
        GetRestrictedCourseDetailsRequest request)
    {
        var filtered = providers;

        if (request.HasProviderNameFilter)
        {
            var searchTerm = request.ProviderName!.Trim();
            filtered = filtered.Where(provider =>
                provider.ProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || provider.Ukprn.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (request.HasDeliveryStatusFilter)
        {
            var selectedStatuses = request.DeliveryStatus.Distinct().ToHashSet();
            filtered = filtered.Where(provider => selectedStatuses.Contains(provider.DeliveryStatus));
        }

        return filtered;
    }

    private static List<FilterItemViewModel> BuildDeliveryStatusItems(GetRestrictedCourseDetailsRequest request)
        =>
        [
            CreateDeliveryStatusItem(
                DeliveryStatus.OpenToNewStarts,
                request,
                "Training providers offer this course on Find apprenticeship training."),
            CreateDeliveryStatusItem(
                DeliveryStatus.LastStartDateAdded,
                request,
                "Training providers cannot accept new learners after this date."),
            CreateDeliveryStatusItem(
                DeliveryStatus.ClosedToNewStarts,
                request,
                "Training providers are no longer allowed to offer this course.")
        ];

    private static FilterItemViewModel CreateDeliveryStatusItem(
        DeliveryStatus status,
        GetRestrictedCourseDetailsRequest request,
        string description)
        => new()
        {
            Value = status.ToString(),
            DisplayText = status.GetDescription(),
            DisplayDescription = description,
            IsSelected = request.DeliveryStatus.Contains(status)
        };
}

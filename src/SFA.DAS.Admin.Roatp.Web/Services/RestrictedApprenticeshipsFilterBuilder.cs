using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public static class RestrictedApprenticeshipsFilterBuilder
{
    public const string RestrictedApprenticeshipFilterResultsFragment = "restricted-apprenticeship-results";

    private const string SearchTermInputId = "search-term-input";
    private const string DeliveryStatusFilterId = "delivery-status-filter";
    private const string LastStartDateAddedDescription = "Course will be restricted after this date";
    private const string ClosedToNewStartsDescription = "This will be removed once all learners have completed the course";

    public static FiltersViewModel CreateFiltersViewModel(
        GetRestrictedApprenticeshipsModel request,
        int ukprn,
        IUrlHelper urlHelper)
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>();
        AddSelectedFilter(selectedFilters, FilterType.SearchTerm, request.SearchTerm?.Trim());
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

        var sectionHeadingOverrides = new Dictionary<FilterType, string>
        {
            [FilterType.SearchTerm] = CourseNameSectionHeading
        };

        var clearFiltersBaseUrl = urlHelper.RouteUrl(RouteNames.ProviderRestrictedCourses, new { ukprn })!;

        return new FiltersViewModel
        {
            Route = RouteNames.ProviderRestrictedCourses,
            Ukprn = ukprn,
            FilterResultsFragment = RestrictedApprenticeshipFilterResultsFragment,
            FilterSections =
            [
                CreateInputFilterSection(
                    SearchTermInputId,
                    CourseNameSectionHeading,
                    CourseNameSectionSubHeading,
                    nameof(FilterType.SearchTerm),
                    request.SearchTerm),
                CreateCheckboxListFilterSection(
                    DeliveryStatusFilterId,
                    nameof(FilterType.DeliveryStatus),
                    DeliveryStatusSectionHeading,
                    null,
                    BuildDeliveryStatusItems(request))
            ],
            ClearFilterSections = CreateClearFilterSections(
                selectedFilters,
                clearFiltersBaseUrl,
                overrideValueFunctions,
                RestrictedApprenticeshipFilterResultsFragment,
                sectionHeadingOverrides)
        };
    }

    public static IEnumerable<RestrictedApprenticeshipItemViewModel> ApplyFilters(
        IEnumerable<RestrictedApprenticeshipItemViewModel> courses,
        GetRestrictedApprenticeshipsModel request)
    {
        var filtered = courses;

        if (request.HasSearchTermFilter)
        {
            var searchTerm = request.SearchTerm.Trim();
            filtered = filtered.Where(course =>
                course.DisplayTitle.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || course.LarsCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (request.HasDeliveryStatusFilter)
        {
            var selectedStatuses = request.DeliveryStatus.Distinct().ToHashSet();
            filtered = filtered.Where(course => selectedStatuses.Contains(course.DeliveryStatus));
        }

        return filtered;
    }

    private static List<FilterItemViewModel> BuildDeliveryStatusItems(GetRestrictedApprenticeshipsModel model)
        =>
        [
            CreateDeliveryStatusItem(DeliveryStatus.LastStartDateAdded, model, LastStartDateAddedDescription),
            CreateDeliveryStatusItem(DeliveryStatus.ClosedToNewStarts, model, ClosedToNewStartsDescription)
        ];

    private static FilterItemViewModel CreateDeliveryStatusItem(
        DeliveryStatus status,
        GetRestrictedApprenticeshipsModel model,
        string description)
        => new()
        {
            Value = status.ToString(),
            DisplayText = status.GetDescription(),
            DisplayDescription = description,
            IsSelected = model.DeliveryStatus.Contains(status)
        };
}

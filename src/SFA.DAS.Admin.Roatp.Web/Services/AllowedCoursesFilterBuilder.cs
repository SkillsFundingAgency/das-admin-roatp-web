using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public static class AllowedCoursesFilterBuilder
{
    public const string AllowedCourseFilterResultsFragment = "allowed-course-results";

    private const string SearchTermInputId = "search-term-input";
    private const string DeliveryStatusFilterId = "delivery-status-filter";
    private const string OpenToNewStartsDescription =
        "Training providers offer this course on Find apprenticeship training";
    private const string LastStartDateAddedDescription =
        "Training providers will not be able to take on new learners after this date";

    public static FiltersViewModel CreateFiltersViewModel(
        GetProviderAllowedCoursesRequestModel requestModel,
        int ukprn,
        RouteLearningType routeLearningType,
        IUrlHelper urlHelper)
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>();
        AddSelectedFilter(selectedFilters, FilterType.SearchTerm, requestModel.SearchTerm?.Trim());
        AddSelectedFilter(
            selectedFilters,
            FilterType.DeliveryStatus,
            requestModel.DeliveryStatus.Distinct().Select(status => status.ToString()));

        var sectionHeadingOverrides = new Dictionary<FilterType, string>
        {
            [FilterType.SearchTerm] = CourseNameSectionHeading
        };

        var clearFiltersBaseUrl = urlHelper.RouteUrl(
            RouteNames.ProviderAllowedCourses,
            new { ukprn, routeLearningType })!;

        return new FiltersViewModel
        {
            Route = RouteNames.ProviderAllowedCourses,
            Ukprn = ukprn,
            RouteLearningType = routeLearningType,
            FilterResultsFragment = AllowedCourseFilterResultsFragment,
            FilterSections =
            [
                CreateInputFilterSection(
                    SearchTermInputId,
                    CourseNameSectionHeading,
                    CourseNameSectionSubHeading,
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
                AllowedCourseFilterResultsFragment,
                sectionHeadingOverrides)
        };
    }

    public static IEnumerable<ProviderAllowedCourseModel> ApplyFilters(
        IEnumerable<ProviderAllowedCourseModel> courses,
        GetProviderAllowedCoursesRequestModel requestModel)
    {
        var filtered = courses;

        if (requestModel.HasSearchTermFilter)
        {
            var searchTerm = requestModel.SearchTerm.Trim();
            filtered = filtered.Where(course =>
                CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level)
                    .Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || course.LarsCode.Equals(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (requestModel.HasDeliveryStatusFilter)
        {
            var selectedStatuses = requestModel.DeliveryStatus.Distinct().ToHashSet();
            filtered = filtered.Where(course => selectedStatuses.Contains(GetDeliveryStatus(course)));
        }

        return filtered;
    }

    private static DeliveryStatus GetDeliveryStatus(ProviderAllowedCourseModel course)
        => course.LastDateStarts.ToAllowedCourseDeliveryStatus(course.IsClosedToNewStarts);

    private static List<FilterItemViewModel> BuildDeliveryStatusItems(
        GetProviderAllowedCoursesRequestModel requestModel)
        =>
        [
            CreateDeliveryStatusItem(DeliveryStatus.OpenToNewStarts, requestModel, OpenToNewStartsDescription),
            CreateDeliveryStatusItem(DeliveryStatus.LastStartDateAdded, requestModel, LastStartDateAddedDescription),
            CreateDeliveryStatusItem(DeliveryStatus.ClosedToNewStarts, requestModel, ClosedToNewStartsDescription)
        ];

    private static FilterItemViewModel CreateDeliveryStatusItem(
        DeliveryStatus status,
        GetProviderAllowedCoursesRequestModel requestModel,
        string description)
        => new()
        {
            Value = status.ToString(),
            DisplayText = status.GetDescription(),
            DisplayDescription = description,
            IsSelected = requestModel.DeliveryStatus.Contains(status)
        };
}

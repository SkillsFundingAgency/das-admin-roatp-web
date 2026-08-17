using System.Text;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.Abstract;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public static class FilterService
{
    public const string ClearFilter = "clearFilter";

    public const string SearchTermSectionHeading = "Provider name";
    public const string SearchTermSectionSubHeading = "Search by name or UKPRN";
    public const string CourseNameSectionHeading = "Course name";
    public const string CourseNameSectionSubHeading = "Search by course name or LARS code";
    public const string DeliveryStatusSectionHeading = "Delivery status";
    public const string LearningTypeSectionHeading = "Training type";

    public enum FilterComponentType
    {
        CheckboxList,
        TextBox
    }

    public enum FilterType
    {
        SearchTerm,
        DeliveryStatus,
        LearningType
    }

    public static Dictionary<FilterType, string> ClearFilterSectionHeadings { get; } = new()
    {
        { FilterType.SearchTerm, SearchTermSectionHeading },
        { FilterType.DeliveryStatus, DeliveryStatusSectionHeading },
        { FilterType.LearningType, LearningTypeSectionHeading }
    };

    public static FilterSection CreateInputFilterSection(
        string id,
        string heading,
        string subHeading,
        string filterFor,
        string? inputValue)
        => new TextBoxFilterSectionViewModel
        {
            Id = id,
            For = filterFor,
            Heading = heading,
            SubHeading = subHeading,
            InputValue = inputValue ?? string.Empty
        };

    public static FilterSection CreateCheckboxListFilterSection(
        string id,
        string filterFor,
        string heading,
        string? subHeading,
        List<FilterItemViewModel> items)
        => new CheckboxListFilterSectionViewModel
        {
            Id = id,
            For = filterFor,
            Heading = heading,
            SubHeading = subHeading ?? string.Empty,
            Items = items
        };

    public static IReadOnlyList<ClearFilterSectionViewModel> CreateClearFilterSections(
        Dictionary<FilterType, IEnumerable<string>> selectedFilters,
        string clearFiltersBaseUrl,
        Dictionary<FilterType, Func<string, string>>? overrideValueFunctions = null,
        string? filterResultsFragment = null,
        Dictionary<FilterType, string>? sectionHeadingOverrides = null)
    {
        if (selectedFilters.Count == 0)
        {
            return [];
        }

        List<ClearFilterSectionViewModel> clearFilterSections = [];

        foreach (var filter in selectedFilters)
        {
            if (!filter.Value.Any())
            {
                continue;
            }

            var title = sectionHeadingOverrides?.GetValueOrDefault(filter.Key)
                        ?? ClearFilterSectionHeadings[filter.Key];

            clearFilterSections.Add(new ClearFilterSectionViewModel
            {
                FilterType = filter.Key,
                Title = title,
                Items = filter.Value.Select(value => new ClearFilterItemViewModel
                {
                    DisplayText = value,
                    ClearLink = BuildClearLink(
                        clearFiltersBaseUrl,
                        filter.Key,
                        value,
                        selectedFilters,
                        overrideValueFunctions,
                        filterResultsFragment)
                }).ToList()
            });
        }

        return clearFilterSections;
    }

    public static void AddSelectedFilter(
        Dictionary<FilterType, IEnumerable<string>> filters,
        FilterType filterType,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            filters[filterType] = [value];
        }
    }

    public static void AddSelectedFilter(
        Dictionary<FilterType, IEnumerable<string>> filters,
        FilterType filterType,
        IEnumerable<string> values)
    {
        var valuesToAdd = values.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        if (valuesToAdd.Count > 0)
        {
            filters[filterType] = valuesToAdd;
        }
    }

    private static string BuildClearLink(
        string clearFiltersBaseUrl,
        FilterType filterType,
        string value,
        Dictionary<FilterType, IEnumerable<string>> queryParams,
        Dictionary<FilterType, Func<string, string>>? overrideValueFunctions,
        string? filterResultsFragment)
    {
        var queryString = BuildQueryWithoutValue(filterType, value, queryParams, overrideValueFunctions);
        var clearLink = string.IsNullOrEmpty(queryString)
            ? clearFiltersBaseUrl
            : $"{clearFiltersBaseUrl}{queryString}";

        return string.IsNullOrWhiteSpace(filterResultsFragment)
            ? clearLink
            : $"{clearLink}#{filterResultsFragment}";
    }

    private static string BuildQueryWithoutValue(
        FilterType filterType,
        string value,
        Dictionary<FilterType, IEnumerable<string>> queryParams,
        Dictionary<FilterType, Func<string, string>>? overrideValueFunctions)
    {
        var queryBuilder = new StringBuilder();

        foreach (var param in queryParams)
        {
            if (!param.Value.Any())
            {
                continue;
            }

            var values = param.Key == filterType
                ? param.Value.Where(v => v != value)
                : param.Value;

            var overrideValueFunction = overrideValueFunctions?.GetValueOrDefault(param.Key);

            foreach (var val in values)
            {
                AppendQueryParam(queryBuilder, param.Key, val, overrideValueFunction);
            }
        }

        return queryBuilder.Length > 0 ? queryBuilder.ToString() : string.Empty;
    }

    private static void AppendQueryParam(
        StringBuilder builder,
        FilterType key,
        string value,
        Func<string, string>? overrideValueFunction = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var queryValue = overrideValueFunction?.Invoke(value) ?? value;
        var encodedValue = Uri.EscapeDataString(queryValue);

        builder
            .Append(builder.Length > 0 ? '&' : '?')
            .Append(key.ToString())
            .Append('=')
            .Append(encodedValue);
    }
}

using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public static class RestrictedCoursesFilterBuilder
{
    private const string SearchTermInputId = "search-term-input";
    private const string LearningTypeFilterId = "learning-type-filter";

    public static FiltersViewModel CreateFiltersViewModel(
        GetRestrictedCoursesRequest request,
        IUrlHelper urlHelper)
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>();
        AddSelectedFilter(selectedFilters, FilterType.SearchTerm, request.SearchTerm?.Trim());

        if (request.HasLearningTypeFilter)
        {
            AddSelectedFilter(
                selectedFilters,
                FilterType.LearningType,
                request.LearningType.Distinct().Select(type => type.GetDescription()));
        }

        var overrideValueFunctions = new Dictionary<FilterType, Func<string, string>>
        {
            [FilterType.LearningType] = displayText =>
                Enum.GetValues<LearningType>()
                    .First(type => type.GetDescription() == displayText)
                    .ToString()
        };

        var sectionHeadingOverrides = new Dictionary<FilterType, string>
        {
            [FilterType.SearchTerm] = CourseNameSectionHeading
        };

        var clearFiltersBaseUrl = urlHelper.RouteUrl(RouteNames.RestrictedCourses)!;

        return new FiltersViewModel
        {
            Route = RouteNames.RestrictedCourses,
            FilterSections =
            [
                CreateInputFilterSection(
                    SearchTermInputId,
                    CourseNameSectionHeading,
                    CourseNameSectionSubHeading,
                    nameof(FilterType.SearchTerm),
                    request.SearchTerm),
                CreateCheckboxListFilterSection(
                    LearningTypeFilterId,
                    nameof(FilterType.LearningType),
                    LearningTypeSectionHeading,
                    null,
                    BuildLearningTypeItems(request))
            ],
            ClearFilterSections = CreateClearFilterSections(
                selectedFilters,
                clearFiltersBaseUrl,
                overrideValueFunctions,
                sectionHeadingOverrides: sectionHeadingOverrides)
        };
    }

    public static IEnumerable<RestrictedCourseItemViewModel> ApplyFilters(
        IEnumerable<RestrictedCourseItemViewModel> courses,
        GetRestrictedCoursesRequest request)
    {
        var filtered = courses;

        if (request.HasSearchTermFilter)
        {
            var searchTerm = request.SearchTerm.Trim();
            filtered = filtered.Where(course =>
                course.DisplayTitle.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || course.LarsCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (request.HasLearningTypeFilter)
        {
            var selectedTypes = request.LearningType.Distinct().ToHashSet();
            filtered = filtered.Where(course => selectedTypes.Contains(course.LearningType));
        }

        return filtered;
    }

    private static List<FilterItemViewModel> BuildLearningTypeItems(GetRestrictedCoursesRequest request)
        =>
        [
            CreateLearningTypeItem(LearningType.Apprenticeship, request),
            CreateLearningTypeItem(LearningType.ApprenticeshipUnit, request),
            CreateLearningTypeItem(LearningType.FoundationApprenticeship, request)
        ];

    private static FilterItemViewModel CreateLearningTypeItem(
        LearningType learningType,
        GetRestrictedCoursesRequest request)
        => new()
        {
            Value = learningType.ToString(),
            DisplayText = learningType.GetDescription(),
            IsSelected = request.LearningType.Contains(learningType)
        };
}

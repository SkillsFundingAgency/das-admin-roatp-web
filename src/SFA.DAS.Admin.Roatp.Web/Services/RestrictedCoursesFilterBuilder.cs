using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public static class RestrictedCoursesFilterBuilder
{
    public const string RestrictedCourseFilterResultsFragment = "restricted-course-results";

    private const string SearchTermInputId = "search-term-input";
    private const string LearningTypeFilterId = "learning-type-filter";

    public static FiltersViewModel CreateFiltersViewModel(
        GetRestrictedCoursesRequestModel requestModel,
        IUrlHelper urlHelper)
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>();
        AddSelectedFilter(selectedFilters, FilterType.SearchTerm, requestModel.SearchTerm?.Trim());

        if (requestModel.HasLearningTypeFilter)
        {
            AddSelectedFilter(
                selectedFilters,
                FilterType.LearningType,
                requestModel.LearningType.Distinct().Select(type => type.ToString()));
        }

        var sectionHeadingOverrides = new Dictionary<FilterType, string>
        {
            [FilterType.SearchTerm] = CourseNameSectionHeading
        };

        var clearFiltersBaseUrl = urlHelper.RouteUrl(RouteNames.RestrictedCourses)!;

        return new FiltersViewModel
        {
            Route = RouteNames.RestrictedCourses,
            FilterResultsFragment = RestrictedCourseFilterResultsFragment,
            FilterSections =
            [
                CreateInputFilterSection(
                    SearchTermInputId,
                    CourseNameSectionHeading,
                    CourseNameSectionSubHeading,
                    nameof(FilterType.SearchTerm),
                    requestModel.SearchTerm),
                CreateCheckboxListFilterSection(
                    LearningTypeFilterId,
                    nameof(FilterType.LearningType),
                    LearningTypeSectionHeading,
                    null,
                    BuildLearningTypeItems(requestModel))
            ],
            ClearFilterSections = CreateClearFilterSections(
                selectedFilters,
                clearFiltersBaseUrl,
                useDisplayText: true,
                RestrictedCourseFilterResultsFragment,
                sectionHeadingOverrides)
        };
    }

    public static IEnumerable<RestrictedCourseModel> ApplyFilters(
        IEnumerable<RestrictedCourseModel> courses,
        GetRestrictedCoursesRequestModel requestModel)
    {
        var filtered = courses;

        if (requestModel.HasSearchTermFilter)
        {
            var searchTerm = requestModel.SearchTerm.Trim();
            filtered = filtered.Where(course =>
                CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level)
                    .Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || course.LarsCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (requestModel.HasLearningTypeFilter)
        {
            var selectedTypes = requestModel.LearningType.Distinct().ToHashSet();
            filtered = filtered.Where(course => selectedTypes.Contains(course.LearningType));
        }

        return filtered;
    }

    private static List<FilterItemViewModel> BuildLearningTypeItems(GetRestrictedCoursesRequestModel requestModel)
        =>
        [
            CreateLearningTypeItem(LearningType.Apprenticeship, requestModel),
            CreateLearningTypeItem(LearningType.ApprenticeshipUnit, requestModel),
            CreateLearningTypeItem(LearningType.FoundationApprenticeship, requestModel)
        ];

    private static FilterItemViewModel CreateLearningTypeItem(
        LearningType learningType,
        GetRestrictedCoursesRequestModel requestModel)
        => new()
        {
            Value = learningType.ToString(),
            DisplayText = learningType.GetDescription(),
            IsSelected = requestModel.LearningType.Contains(learningType)
        };
}

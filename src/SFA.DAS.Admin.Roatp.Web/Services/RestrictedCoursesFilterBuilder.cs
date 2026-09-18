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
        GetRestrictedCoursesModel model,
        IUrlHelper urlHelper)
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>();
        AddSelectedFilter(selectedFilters, FilterType.SearchTerm, model.SearchTerm?.Trim());

        if (model.HasLearningTypeFilter)
        {
            AddSelectedFilter(
                selectedFilters,
                FilterType.LearningType,
                model.LearningType.Distinct().Select(type => type.ToString()));
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
                    model.SearchTerm),
                CreateCheckboxListFilterSection(
                    LearningTypeFilterId,
                    nameof(FilterType.LearningType),
                    LearningTypeSectionHeading,
                    null,
                    BuildLearningTypeItems(model))
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
        GetRestrictedCoursesModel model)
    {
        var filtered = courses;

        if (model.HasSearchTermFilter)
        {
            var searchTerm = model.SearchTerm.Trim();
            filtered = filtered.Where(course =>
                CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level)
                    .Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || course.LarsCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (model.HasLearningTypeFilter)
        {
            var selectedTypes = model.LearningType.Distinct().ToHashSet();
            filtered = filtered.Where(course => selectedTypes.Contains(course.LearningType));
        }

        return filtered;
    }

    private static List<FilterItemViewModel> BuildLearningTypeItems(GetRestrictedCoursesModel model)
        =>
        [
            CreateLearningTypeItem(LearningType.Apprenticeship, model),
            CreateLearningTypeItem(LearningType.ApprenticeshipUnit, model),
            CreateLearningTypeItem(LearningType.FoundationApprenticeship, model)
        ];

    private static FilterItemViewModel CreateLearningTypeItem(
        LearningType learningType,
        GetRestrictedCoursesModel model)
        => new()
        {
            Value = learningType.ToString(),
            DisplayText = learningType.GetDescription(),
            IsSelected = model.LearningType.Contains(learningType)
        };
}

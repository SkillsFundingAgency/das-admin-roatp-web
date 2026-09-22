using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Services;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class RestrictedCoursesFilterBuilderTests
{
    private const string RestrictedCoursesUrl = "/restricted-courses";

    [Test]
    public void WhenApplyingCourseNameFilteryName_ThenMatchesDisplayTitle()
    {
        var courses = CreateCourses();

        var byName = RestrictedCoursesFilterBuilder.ApplyFilters(
            courses,
            new GetRestrictedCoursesRequestModel { SearchTerm = "Cleaning" }).ToList();

        byName.Should().ContainSingle(c => c.Title == "Cleaning hygiene operative");
    }

    [Test]
    public void WhenApplyingCourseNameFilterByLarsCode_ThenMatcheLarsCode()
    {
        var courses = CreateCourses();

        var byLarsCode = RestrictedCoursesFilterBuilder.ApplyFilters(
            courses,
            new GetRestrictedCoursesRequestModel { SearchTerm = "124" }).ToList();

        byLarsCode.Should().ContainSingle(c => c.LarsCode == "124");
    }

    [Test]
    public void WhenApplyingCourseNameFilter_AndLarsCodeIsPartial_ThenDoesNotMatch()
    {
        var courses = CreateCourses();

        var filtered = RestrictedCoursesFilterBuilder.ApplyFilters(
            courses,
            new GetRestrictedCoursesRequestModel { SearchTerm = "12" }).ToList();

        filtered.Should().BeEmpty();
    }

    [Test]
    public void WhenApplyingLearningTypeFilter_ThenMatchesSelectedTypes()
    {
        var courses = CreateCourses();

        var filtered = RestrictedCoursesFilterBuilder.ApplyFilters(
            courses,
            new GetRestrictedCoursesRequestModel
            {
                LearningType = [LearningType.ApprenticeshipUnit]
            }).ToList();

        filtered.Should().ContainSingle(c => c.LearningType == LearningType.ApprenticeshipUnit);
    }

    [Test]
    public void WhenApplyingAllLearningTypes_ThenReturnsAllCourses()
    {
        var courses = CreateCourses();

        var filtered = RestrictedCoursesFilterBuilder.ApplyFilters(
            courses,
            new GetRestrictedCoursesRequestModel
            {
                LearningType =
                [
                    LearningType.Apprenticeship,
                    LearningType.ApprenticeshipUnit,
                    LearningType.FoundationApprenticeship
                ]
            }).ToList();

        filtered.Should().HaveCount(courses.Count);
    }

    [Test]
    public void WhenApplyingNoFilters_ThenReturnsAllCourses()
    {
        var courses = CreateCourses();

        var filtered = RestrictedCoursesFilterBuilder.ApplyFilters(
            courses,
            new GetRestrictedCoursesRequestModel()).ToList();

        filtered.Should().HaveCount(courses.Count);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndSearchTermHasSurroundingWhitespace_ThenTrimsSearchTermForClearLink()
    {
        var urlHelper = CreateUrlHelper();
        var requestModel = new GetRestrictedCoursesRequestModel
        {
            SearchTerm = "  Paint  "
        };

        var filters = RestrictedCoursesFilterBuilder.CreateFiltersViewModel(requestModel, urlHelper.Object);

        filters.ClearFilterSections.Single().Items.Single().DisplayText.Should().Be("Paint");
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndFiltersAreApplied_ThenShowFilterOptionsIsTrue()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        filters.ShowFilterOptions.Should().BeTrue();
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenDoesNotSetLarsCode()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        filters.LarsCode.Should().BeNull();
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenSetsFilterResultsFragment()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        filters.FilterResultsFragment.Should().Be(RestrictedCoursesFilterBuilder.RestrictedCourseFilterResultsFragment);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenHasTwoFilterSections()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        filters.FilterSections.Should().HaveCount(2);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenBuildsCourseNameSection()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        using (new AssertionScope())
        {
            var searchSection = filters.FilterSections[0].Should().BeOfType<TextBoxFilterSectionViewModel>().Subject;
            searchSection.Heading.Should().Be(CourseNameSectionHeading);
            searchSection.SubHeading.Should().Be(CourseNameSectionSubHeading);
            searchSection.InputValue.Should().Be("Paint");
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenBuildsLearningTypeSection()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        using (new AssertionScope())
        {
            var learningTypeSection = filters.FilterSections[1]
                .Should().BeOfType<CheckboxListFilterSectionViewModel>().Subject;
            learningTypeSection.Heading.Should().Be(LearningTypeSectionHeading);
            learningTypeSection.Items.Should().HaveCount(3);
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenMarksSelectedLearningType()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        using (new AssertionScope())
        {
            var learningTypeSection = filters.FilterSections[1]
                .Should().BeOfType<CheckboxListFilterSectionViewModel>().Subject;
            learningTypeSection.Items.Single(i => i.Value == nameof(LearningType.ApprenticeshipUnit))
                .IsSelected.Should().BeTrue();
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenBuildsClearFilterSections()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        using (new AssertionScope())
        {
            filters.ClearFilterSections.Should().HaveCount(2);
            filters.ClearFilterSections.Should().Contain(section =>
                section.Title == CourseNameSectionHeading
                && section.Items.Single().DisplayText == "Paint");
            filters.ClearFilterSections.Should().Contain(section =>
                section.Title == LearningTypeSectionHeading
                && section.Items.Single().DisplayText == "Apprenticeship unit");
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenClearCourseNameLinkKeepsLearningType()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        var clearCourseNameLink = filters.ClearFilterSections
            .Single(section => section.Title == CourseNameSectionHeading)
            .Items.Single().ClearLink;

        clearCourseNameLink.Should().Be(
            $"{RestrictedCoursesUrl}?LearningType=ApprenticeshipUnit#{RestrictedCoursesFilterBuilder.RestrictedCourseFilterResultsFragment}");
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndAllLearningTypesSelected_ThenShowsLearningTypesInSelectedFilters()
    {
        var urlHelper = CreateUrlHelper();
        var requestModel = new GetRestrictedCoursesRequestModel
        {
            LearningType =
            [
                LearningType.Apprenticeship,
                LearningType.ApprenticeshipUnit,
                LearningType.FoundationApprenticeship
            ]
        };

        var filters = RestrictedCoursesFilterBuilder.CreateFiltersViewModel(requestModel, urlHelper.Object);

        using (new AssertionScope())
        {
            filters.ShowFilterOptions.Should().BeTrue();

            var learningTypeClearSection = filters.ClearFilterSections
                .Should().ContainSingle(section => section.Title == LearningTypeSectionHeading)
                .Subject;

            learningTypeClearSection.Items.Select(item => item.DisplayText).Should().BeEquivalentTo(
                "Apprenticeship",
                "Apprenticeship unit",
                "Foundation apprenticeship");

            var learningTypeSection = filters.FilterSections[1].Should().BeOfType<CheckboxListFilterSectionViewModel>().Subject;
            learningTypeSection.Items.Should().OnlyContain(i => i.IsSelected);
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndClearingLastFilter_ThenClearLinkIsBaseUrl()
    {
        var urlHelper = CreateUrlHelper();
        var requestModel = new GetRestrictedCoursesRequestModel
        {
            SearchTerm = "Paint"
        };

        var filters = RestrictedCoursesFilterBuilder.CreateFiltersViewModel(requestModel, urlHelper.Object);

        filters.ClearFilterSections.Single().Items.Single().ClearLink.Should().Be(
            $"{RestrictedCoursesUrl}#{RestrictedCoursesFilterBuilder.RestrictedCourseFilterResultsFragment}");
    }

    private static FiltersViewModel CreateFiltersViewModelWithSelectedFilters()
    {
        var requestModel = new GetRestrictedCoursesRequestModel
        {
            SearchTerm = "Paint",
            LearningType = [LearningType.ApprenticeshipUnit]
        };

        return RestrictedCoursesFilterBuilder.CreateFiltersViewModel(requestModel, CreateUrlHelper().Object);
    }

    private static List<RestrictedCourseModel> CreateCourses() =>
    [
        new()
        {
            LarsCode = "124",
            Title = "Chartered manager",
            Level = 6,
            LearningType = LearningType.Apprenticeship
        },
        new()
        {
            LarsCode = "163",
            Title = "Cleaning hygiene operative",
            Level = 2,
            LearningType = LearningType.Apprenticeship
        },
        new()
        {
            LarsCode = "999",
            Title = "Paint unit",
            Level = 3,
            LearningType = LearningType.ApprenticeshipUnit
        }
    ];

    private static Mock<IUrlHelper> CreateUrlHelper()
    {
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Returns((UrlRouteContext context) =>
            {
                context.RouteName.Should().Be(RouteNames.RestrictedCourses);
                return RestrictedCoursesUrl;
            });
        return urlHelper;
    }
}

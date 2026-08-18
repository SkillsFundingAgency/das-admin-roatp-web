using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class RestrictedCoursesFilterBuilderTests
{
    private const string RestrictedCoursesUrl = "/restricted-courses";

    [Test]
    public void WhenApplyingCourseNameFilter_ThenMatchesDisplayTitleOrLarsCode()
    {
        var courses = CreateCourses();

        var byName = RestrictedCoursesFilterBuilder.ApplyFilters(
            courses,
            new GetRestrictedCoursesRequest { SearchTerm = "cleaning" }).ToList();

        byName.Should().ContainSingle(c => c.LarsCode == "163");

        var byLarsCode = RestrictedCoursesFilterBuilder.ApplyFilters(
            courses,
            new GetRestrictedCoursesRequest { SearchTerm = "124" }).ToList();

        byLarsCode.Should().ContainSingle(c => c.LarsCode == "124");
    }

    [Test]
    public void WhenApplyingLearningTypeFilter_ThenMatchesSelectedTypes()
    {
        var courses = CreateCourses();

        var filtered = RestrictedCoursesFilterBuilder.ApplyFilters(
            courses,
            new GetRestrictedCoursesRequest
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
            new GetRestrictedCoursesRequest
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
            new GetRestrictedCoursesRequest()).ToList();

        filtered.Should().HaveCount(courses.Count);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndSearchTermHasSurroundingWhitespace_ThenTrimsSearchTermForClearLink()
    {
        var urlHelper = CreateUrlHelper();
        var request = new GetRestrictedCoursesRequest
        {
            SearchTerm = "  Paint  "
        };

        var filters = RestrictedCoursesFilterBuilder.CreateFiltersViewModel(request, urlHelper.Object);

        filters.ClearFilterSections.Single().Items.Single().DisplayText.Should().Be("Paint");
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenBuildsSectionsAndClearLinks()
    {
        var urlHelper = CreateUrlHelper();
        var request = new GetRestrictedCoursesRequest
        {
            SearchTerm = "Paint",
            LearningType = [LearningType.ApprenticeshipUnit]
        };

        var filters = RestrictedCoursesFilterBuilder.CreateFiltersViewModel(request, urlHelper.Object);

        filters.ShowFilterOptions.Should().BeTrue();
        filters.LarsCode.Should().BeNull();
        filters.FilterResultsFragment.Should().Be(RestrictedCoursesFilterBuilder.RestrictedCourseFilterResultsFragment);
        filters.FilterSections.Should().HaveCount(2);

        var searchSection = filters.FilterSections[0].Should().BeOfType<TextBoxFilterSectionViewModel>().Subject;
        searchSection.Heading.Should().Be(CourseNameSectionHeading);
        searchSection.SubHeading.Should().Be(CourseNameSectionSubHeading);
        searchSection.InputValue.Should().Be("Paint");

        var learningTypeSection = filters.FilterSections[1].Should().BeOfType<CheckboxListFilterSectionViewModel>().Subject;
        learningTypeSection.Heading.Should().Be(LearningTypeSectionHeading);
        learningTypeSection.Items.Should().HaveCount(3);
        learningTypeSection.Items.Single(i => i.Value == nameof(LearningType.ApprenticeshipUnit)).IsSelected.Should().BeTrue();

        filters.ClearFilterSections.Should().HaveCount(2);
        filters.ClearFilterSections.Should().Contain(section =>
            section.Title == CourseNameSectionHeading
            && section.Items.Single().DisplayText == "Paint");
        filters.ClearFilterSections.Should().Contain(section =>
            section.Title == LearningTypeSectionHeading
            && section.Items.Single().DisplayText == "Apprenticeship unit");

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
        var request = new GetRestrictedCoursesRequest
        {
            LearningType =
            [
                LearningType.Apprenticeship,
                LearningType.ApprenticeshipUnit,
                LearningType.FoundationApprenticeship
            ]
        };

        var filters = RestrictedCoursesFilterBuilder.CreateFiltersViewModel(request, urlHelper.Object);

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

    [Test]
    public void WhenCreatingFiltersViewModel_AndClearingLastFilter_ThenClearLinkIsBaseUrl()
    {
        var urlHelper = CreateUrlHelper();
        var request = new GetRestrictedCoursesRequest
        {
            SearchTerm = "Paint"
        };

        var filters = RestrictedCoursesFilterBuilder.CreateFiltersViewModel(request, urlHelper.Object);

        filters.ClearFilterSections.Single().Items.Single().ClearLink.Should().Be(
            $"{RestrictedCoursesUrl}#{RestrictedCoursesFilterBuilder.RestrictedCourseFilterResultsFragment}");
    }

    private static List<RestrictedCourseItemViewModel> CreateCourses() =>
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

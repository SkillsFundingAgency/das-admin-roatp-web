using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class RestrictedCoursesControllerGetTests
{
    private const string RestrictedCoursesUrl = "/restricted-courses";

    [Test, MoqAutoData]
    public async Task Index_ReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCoursesController sut)
    {
        var response = new GetRestrictedCoursesResponse
        {
            Courses =
            [
                new RestrictedCourseModel
                {
                    LarsCode = "124",
                    Title = "Bricklaying",
                    Level = 2,
                    LearningType = LearningType.Apprenticeship
                }
            ]
        };

        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(new GetRestrictedCoursesRequest(), CancellationToken.None) as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be(RestrictedCoursesController.ViewPath);
        var model = result.Model as RestrictedCoursesViewModel;
        model.Should().NotBeNull();
        model!.TotalCount.Should().Be(response.Courses.Count);
        model.TotalCountDescription.Should().Be("1 course");
        model.HasCourses.Should().BeTrue();
        model.ShowCourseResults.Should().BeTrue();
        model.HasActiveFilters.Should().BeFalse();
        model.Filters.FilterSections.Should().HaveCount(2);
        model.Courses.Should().HaveCount(1);
        var course = model.Courses.Single();
        course.LarsCode.Should().Be("124");
        course.DisplayTitle.Should().Be("Bricklaying (Level 2)");
        course.LearningTypeDescription.Should().Be("Apprenticeship");
        course.LearningTypeTagClass.Should().Be("govuk-tag--blue");

        outerApiClientMock.Verify(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Index_WhenSearchTermFilterApplied_ThenReturnsMatchingCoursesOrderedAlphabetically(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCoursesController sut)
    {
        var response = new GetRestrictedCoursesResponse
        {
            Courses =
            [
                new RestrictedCourseModel
                {
                    LarsCode = "200",
                    Title = "Zebra course",
                    Level = 2,
                    LearningType = LearningType.Apprenticeship
                },
                new RestrictedCourseModel
                {
                    LarsCode = "100",
                    Title = "Alpha course",
                    Level = 3,
                    LearningType = LearningType.Apprenticeship
                },
                new RestrictedCourseModel
                {
                    LarsCode = "300",
                    Title = "Other course",
                    Level = 4,
                    LearningType = LearningType.ApprenticeshipUnit
                }
            ]
        };

        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(
            new GetRestrictedCoursesRequest { SearchTerm = "course" },
            CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCoursesViewModel;
        model!.HasActiveFilters.Should().BeTrue();
        model.HasNoFilterResults.Should().BeFalse();
        model.TotalCount.Should().Be(3);
        model.Courses.Select(c => c.DisplayTitle).Should().ContainInOrder(
            "Alpha course (Level 3)",
            "Other course (Level 4)",
            "Zebra course (Level 2)");
    }

    [Test, MoqAutoData]
    public async Task Index_WhenFiltersReturnNoResults_ThenShowsNoFilterResultsState(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCoursesController sut)
    {
        var response = new GetRestrictedCoursesResponse
        {
            Courses =
            [
                new RestrictedCourseModel
                {
                    LarsCode = "124",
                    Title = "Bricklaying",
                    Level = 2,
                    LearningType = LearningType.Apprenticeship
                }
            ]
        };

        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(
            new GetRestrictedCoursesRequest { SearchTerm = "nomatch" },
            CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCoursesViewModel;
        model!.HasActiveFilters.Should().BeTrue();
        model.HasCourses.Should().BeFalse();
        model.HasNoCourses.Should().BeFalse();
        model.HasNoFilterResults.Should().BeTrue();
        model.ShowCourseResults.Should().BeTrue();
        model.TotalCount.Should().Be(0);
        model.Filters.ShowFilterOptions.Should().BeTrue();
    }
}

using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageCourses;

[TestFixture]
public class RestrictedCoursesViewModelTests
{
    [Test]
    public void ImplicitConversion_MapsResponseToViewModel()
    {
        var response = new GetRestrictedCoursesResponse
        {
            Courses =
            [
                new RestrictedCourseModel
                {
                    LarsCode = "163",
                    Title = "Business Administrator",
                    Level = 4,
                    LearningType = LearningType.Apprenticeship
                }
            ]
        };

        RestrictedCoursesViewModel model = response;

        model.TotalCount.Should().Be(response.Courses.Count);
        model.TotalCountDescription.Should().Be("1 course");
        model.HasCourses.Should().BeTrue();
        model.HasNoCourses.Should().BeFalse();
        model.Courses.Should().ContainSingle();
        var course = model.Courses.Single();
        course.LarsCode.Should().Be("163");
        course.Title.Should().Be("Business Administrator");
        course.Level.Should().Be(4);
        course.LearningType.Should().Be(LearningType.Apprenticeship);
        course.DisplayTitle.Should().Be("Business Administrator (Level 4)");
    }

    [Test]
    public void ImplicitConversion_MapsEmptyResponseToViewModel()
    {
        var response = new GetRestrictedCoursesResponse();

        RestrictedCoursesViewModel model = response;

        model.TotalCount.Should().Be(response.Courses.Count);
        model.TotalCountDescription.Should().Be("0 courses");
        model.HasCourses.Should().BeFalse();
        model.HasNoCourses.Should().BeTrue();
        model.Courses.Should().BeEmpty();
    }

    [Test]
    public void ImplicitConversion_MapsNullResponseToEmptyViewModel()
    {
        GetRestrictedCoursesResponse? response = null;

        RestrictedCoursesViewModel model = response!;

        model.TotalCount.Should().Be(0);
        model.TotalCountDescription.Should().Be("0 courses");
        model.HasCourses.Should().BeFalse();
        model.HasNoCourses.Should().BeTrue();
        model.Courses.Should().BeEmpty();
    }

    [Test]
    public void TotalCountDescription_ReturnsSingularTextForOneCourse()
    {
        var model = new RestrictedCoursesViewModel
        {
            TotalCount = 1
        };

        model.TotalCountDescription.Should().Be("1 course");
    }

    [Test]
    public void TotalCountDescription_ReturnsPluralTextForMultipleCourses()
    {
        var model = new RestrictedCoursesViewModel
        {
            TotalCount = 2
        };

        model.TotalCountDescription.Should().Be("2 courses");
    }

    [Test]
    public void HasCourses_ReturnsTrueWhenCoursesArePresent()
    {
        var model = new RestrictedCoursesViewModel
        {
            Courses =
            [
                new RestrictedCourseItemViewModel
                {
                    LarsCode = "163",
                    Title = "Business Administrator",
                    Level = 4,
                    LearningType = LearningType.Apprenticeship
                }
            ]
        };

        model.HasCourses.Should().BeTrue();
        model.HasNoCourses.Should().BeFalse();
    }

    [Test]
    public void HasCourses_ReturnsFalseWhenNoCoursesArePresent()
    {
        var model = new RestrictedCoursesViewModel();

        model.HasCourses.Should().BeFalse();
        model.HasNoCourses.Should().BeTrue();
    }
}

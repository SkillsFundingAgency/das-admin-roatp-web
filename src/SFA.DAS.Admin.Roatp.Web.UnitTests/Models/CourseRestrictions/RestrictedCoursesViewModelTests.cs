using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.CourseRestrictions;

[TestFixture]
public class RestrictedCoursesViewModelTests
{
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
            TotalCount = 1,
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

        using (new AssertionScope())
        {
            model.HasCourses.Should().BeTrue();
            model.HasNoCourses.Should().BeFalse();
        }
    }

    [Test]
    public void HasCourses_ReturnsFalseWhenNoCoursesArePresent()
    {
        var model = new RestrictedCoursesViewModel();

        using (new AssertionScope())
        {
            model.HasCourses.Should().BeFalse();
            model.HasNoCourses.Should().BeTrue();
            model.HasNoFilteredResults.Should().BeFalse();
            model.ShowCourseResults.Should().BeFalse();
        }
    }

    [Test]
    public void HasNoFilteredResults_WhenActiveFiltersAndNoCourses_ThenIsTrue()
    {
        var model = new RestrictedCoursesViewModel
        {
            HasActiveFilters = true
        };

        using (new AssertionScope())
        {
            model.HasNoFilteredResults.Should().BeTrue();
            model.HasNoCourses.Should().BeFalse();
            model.ShowCourseResults.Should().BeTrue();
        }
    }
}

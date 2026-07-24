using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.RestrictedCourses;

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
        model.Courses.Should().ContainSingle();
        model.Courses[0].LarsCode.Should().Be("163");
        model.Courses[0].Title.Should().Be("Business Administrator");
        model.Courses[0].Level.Should().Be(4);
        model.Courses[0].LearningType.Should().Be(LearningType.Apprenticeship);
        model.Courses[0].DisplayTitle.Should().Be("Business Administrator (Level 4)");
    }

    [Test]
    public void ImplicitConversion_MapsEmptyResponseToViewModel()
    {
        var response = new GetRestrictedCoursesResponse();


        RestrictedCoursesViewModel model = response;

        model.TotalCount.Should().Be(response.Courses.Count);
        model.TotalCountDescription.Should().Be("0 courses");
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
}

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
            TotalCount = 1,
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

        model.TotalCount.Should().Be(1);
        model.TotalCountDescription.Should().Be("1 course");
        model.Courses.Should().ContainSingle();
        model.Courses[0].LarsCode.Should().Be("163");
        model.Courses[0].Title.Should().Be("Business Administrator");
        model.Courses[0].Level.Should().Be(4);
        model.Courses[0].LearningType.Should().Be(LearningType.Apprenticeship);
        model.Courses[0].DisplayTitle.Should().Be("Business Administrator (Level 4)");
    }
}

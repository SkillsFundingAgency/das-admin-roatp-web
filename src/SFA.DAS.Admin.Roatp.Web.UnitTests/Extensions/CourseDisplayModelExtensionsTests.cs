using FluentAssertions;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;

[TestFixture]
public class CourseDisplayModelExtensionsTests
{
    [TestCase("Bricklaying", 2, "Bricklaying (Level 2)")]
    [TestCase("Business Administrator", 4, "Business Administrator (Level 4)")]
    public void GetDisplayTitle_ReturnsExpectedFormat(string title, int level, string expected)
    {
        var course = new TestCourseDisplayModel
        {
            Title = title,
            Level = level
        };

        course.GetDisplayTitle().Should().Be(expected);
    }

    private class TestCourseDisplayModel : ICourseDisplayModel
    {
        public string Title { get; set; } = string.Empty;
        public int Level { get; set; }
    }
}

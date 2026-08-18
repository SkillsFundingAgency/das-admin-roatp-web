using FluentAssertions;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageCourses;

[TestFixture]
public class AddLastDateStartsViewModelTests
{
    private const string LarsCode = "105";

    [Test]
    public void CourseLastDateStartsText_WhenDateIsNull_ThenReturnsEmptyString()
    {
        var model = new AddLastDateStartsViewModel
        {
            LarsCode = LarsCode,
            ProviderName = "BP TRAINING",
            CourseDisplayTitle = "Academic professional (Level 7)",
            CourseLastDateStarts = null
        };

        model.CourseLastDateStartsText.Should().BeEmpty();
    }

    [Test]
    public void CourseLastDateStartsText_WhenDateHasValue_ThenReturnsFormattedDate()
    {
        var date = new DateTime(2027, 6, 1);
        var model = new AddLastDateStartsViewModel
        {
            LarsCode = LarsCode,
            ProviderName = "BP TRAINING",
            CourseDisplayTitle = "Academic professional (Level 7)",
            CourseLastDateStarts = date
        };

        model.CourseLastDateStartsText.Should().Be(date.ToScreenString());
    }
}

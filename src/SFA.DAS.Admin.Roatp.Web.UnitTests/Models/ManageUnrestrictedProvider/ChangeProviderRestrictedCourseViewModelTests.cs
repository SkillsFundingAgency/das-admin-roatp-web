using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageUnrestrictedProvider;

[TestFixture]
public class ChangeProviderRestrictedCourseViewModelTests
{
    [Test]
    public void WhenLastDateStartsExists_ThenHasLastDateStartsIsTrueAndTextIsFormatted()
    {
        var model = new ChangeProviderRestrictedCourseViewModel
        {
            LarsCode = "105",
            DisplayTitle = "Electrical (Level 3)",
            LastDateStarts = new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified),
            CancelUrl = "/cancel"
        };

        using (new AssertionScope())
        {
            model.HasLastDateStarts.Should().BeTrue();
            model.LastDateStartsText.Should().Be("12 Jul 2026");
        }
    }

    [Test]
    public void WhenLastDateStartsDoesNotExist_ThenHasLastDateStartsIsFalse()
    {
        var model = new ChangeProviderRestrictedCourseViewModel
        {
            LarsCode = "105",
            DisplayTitle = "Electrical (Level 3)",
            LastDateStarts = null,
            CancelUrl = "/cancel"
        };

        using (new AssertionScope())
        {
            model.HasLastDateStarts.Should().BeFalse();
            model.LastDateStartsText.Should().BeNull();
        }
    }
}

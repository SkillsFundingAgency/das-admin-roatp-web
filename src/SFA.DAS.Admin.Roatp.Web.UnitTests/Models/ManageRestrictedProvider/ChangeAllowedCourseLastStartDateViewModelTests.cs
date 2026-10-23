using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageRestrictedProvider;

[TestFixture]
public class ChangeAllowedCourseLastStartDateViewModelTests
{
    [Test]
    public void WhenLastDateStartsExists_ThenTextIsFormatted()
    {
        var model = new ChangeAllowedCourseLastStartDateViewModel
        {
            Ukprn = 10019900,
            RouteLearningType = RouteLearningType.Apprenticeship,
            LarsCode = "105",
            CourseDisplayTitle = "Carpentry (Level 1)",
            LastDateStarts = new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified),
            CancelUrl = "/cancel"
        };

        model.LastDateStartsText.Should().Be("12 Jul 2026");
    }
}

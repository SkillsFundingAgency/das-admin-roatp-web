using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageRestrictedProvider;

[TestFixture]
public class AllowedCourseItemViewModelTests
{
    [Test]
    public void WhenMappingFromCourse_AndNotClosedWithNoLastDateStarts_ThenMapsOpenToNewStarts()
    {
        var course = CreateCourse(lastDateStarts: null, isClosedToNewStarts: false);

        AllowedCourseItemViewModel model = course;

        using (new AssertionScope())
        {
            model.DisplayTitle.Should().Be("Chartered manager (Level 6)");
            model.DeliveryStatus.Should().Be(DeliveryStatus.OpenToNewStarts);
            model.DeliveryStatusDescription.Should().Be("Open to new starts");
            model.DeliveryStatusTagClass.Should().Be("govuk-tag--green");
        }
    }

    [Test]
    public void WhenMappingFromCourse_AndNotClosedWithFutureLastDateStarts_ThenMapsLastStartDateAdded()
    {
        var course = CreateCourse(DateTime.UtcNow.Date.AddDays(10), isClosedToNewStarts: false);

        AllowedCourseItemViewModel model = course;

        using (new AssertionScope())
        {
            model.DeliveryStatus.Should().Be(DeliveryStatus.LastStartDateAdded);
            model.DeliveryStatusTagClass.Should().Be("govuk-tag--orange");
        }
    }

    [Test]
    public void WhenMappingFromCourse_AndIsClosedToNewStarts_ThenMapsClosedToNewStarts()
    {
        var course = CreateCourse(DateTime.UtcNow.Date.AddDays(10), isClosedToNewStarts: true);

        AllowedCourseItemViewModel model = course;

        using (new AssertionScope())
        {
            model.DeliveryStatus.Should().Be(DeliveryStatus.ClosedToNewStarts);
            model.DeliveryStatusTagClass.Should().Be("govuk-tag--grey");
        }
    }

    [Test]
    public void WhenMappingFromCourse_AndNotClosedWithPastLastDateStarts_ThenMapsClosedToNewStarts()
    {
        var course = CreateCourse(DateTime.UtcNow.Date.AddDays(-1), isClosedToNewStarts: false);

        AllowedCourseItemViewModel model = course;

        model.DeliveryStatus.Should().Be(DeliveryStatus.ClosedToNewStarts);
    }

    private static ProviderAllowedCourseModel CreateCourse(DateTime? lastDateStarts, bool isClosedToNewStarts)
        => new()
        {
            LarsCode = "105",
            Title = "Chartered manager",
            Level = 6,
            LastDateStarts = lastDateStarts,
            IsClosedToNewStarts = isClosedToNewStarts
        };
}

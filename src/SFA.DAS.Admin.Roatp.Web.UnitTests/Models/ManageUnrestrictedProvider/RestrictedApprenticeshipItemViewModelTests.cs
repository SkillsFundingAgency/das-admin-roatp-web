using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageUnrestrictedProvider;

[TestFixture]
public class RestrictedApprenticeshipItemViewModelTests
{
    [Test]
    public void WhenMappingFromCourse_AndIsClosedToNewStartsWithLastDateStarts_ThenMapsClosedToNewStarts()
    {
        var course = new RestrictedApprenticeshipModel
        {
            LarsCode = "105",
            Title = "Chartered manager",
            Level = 6,
            LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
            IsClosedToNewStarts = true
        };

        RestrictedApprenticeshipItemViewModel model = course;

        using (new AssertionScope())
        {
            model.LarsCode.Should().Be(course.LarsCode);
            model.Title.Should().Be(course.Title);
            model.Level.Should().Be(course.Level);
            model.IsClosedToNewStarts.Should().BeTrue();
            model.DisplayTitle.Should().Be("Chartered manager (Level 6)");
            model.DeliveryStatus.Should().Be(DeliveryStatus.ClosedToNewStarts);
            model.DeliveryStatusDescription.Should().Be("Closed to new starts");
            model.DeliveryStatusTagClass.Should().Be("govuk-tag--grey");
        }
    }

    [Test]
    public void WhenMappingFromCourse_AndIsClosedToNewStartsWithNoLastDateStarts_ThenMapsClosedToNewStarts()
    {
        var course = new RestrictedApprenticeshipModel
        {
            LarsCode = "105",
            Title = "Chartered manager",
            Level = 6,
            LastDateStarts = null,
            IsClosedToNewStarts = true
        };

        RestrictedApprenticeshipItemViewModel model = course;

        using (new AssertionScope())
        {
            model.IsClosedToNewStarts.Should().BeTrue();
            model.DeliveryStatus.Should().Be(DeliveryStatus.ClosedToNewStarts);
            model.DeliveryStatusDescription.Should().Be("Closed to new starts");
        }
    }

    [Test]
    public void WhenMappingFromCourse_AndIsClosedToNewStartsWithLastDateStartsToday_ThenMapsClosedToNewStarts()
    {
        var course = new RestrictedApprenticeshipModel
        {
            LarsCode = "105",
            Title = "Chartered manager",
            Level = 6,
            LastDateStarts = DateTime.UtcNow.Date,
            IsClosedToNewStarts = true
        };

        RestrictedApprenticeshipItemViewModel model = course;

        using (new AssertionScope())
        {
            model.IsClosedToNewStarts.Should().BeTrue();
            model.DeliveryStatus.Should().Be(DeliveryStatus.ClosedToNewStarts);
        }
    }

    [Test]
    public void WhenMappingFromCourse_AndNotClosedToNewStartsWithFutureLastDateStarts_ThenMapsLastStartDateAdded()
    {
        var course = new RestrictedApprenticeshipModel
        {
            LarsCode = "105",
            Title = "Chartered manager",
            Level = 6,
            LastDateStarts = DateTime.UtcNow.Date.AddDays(10),
            IsClosedToNewStarts = false
        };

        RestrictedApprenticeshipItemViewModel model = course;

        using (new AssertionScope())
        {
            model.IsClosedToNewStarts.Should().BeFalse();
            model.DeliveryStatus.Should().Be(DeliveryStatus.LastStartDateAdded);
            model.DeliveryStatusDescription.Should().Be("Last start date added");
            model.DeliveryStatusTagClass.Should().Be("govuk-tag--orange");
        }
    }

    [Test]
    public void WhenMappingFromCourse_AndNotClosedToNewStartsWithNoLastDateStarts_ThenMapsOpenToNewStarts()
    {
        var course = new RestrictedApprenticeshipModel
        {
            LarsCode = "105",
            Title = "Chartered manager",
            Level = 6,
            LastDateStarts = null,
            IsClosedToNewStarts = false
        };

        RestrictedApprenticeshipItemViewModel model = course;

        using (new AssertionScope())
        {
            model.IsClosedToNewStarts.Should().BeFalse();
            model.DeliveryStatus.Should().Be(DeliveryStatus.OpenToNewStarts);
            model.DeliveryStatusDescription.Should().Be("Open to new starts");
        }
    }
}

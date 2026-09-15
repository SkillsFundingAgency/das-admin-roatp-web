using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;

[TestFixture]
public class EnumExtensionsDeliveryStatusTests
{
    [TestCase(DeliveryStatus.OpenToNewStarts, "govuk-tag--green")]
    [TestCase(DeliveryStatus.LastStartDateAdded, "govuk-tag--orange")]
    [TestCase(DeliveryStatus.ClosedToNewStarts, "govuk-tag--grey")]
    public void WhenGettingTagClass_AndDeliveryStatusProvided_ThenReturnsExpectedClass(DeliveryStatus deliveryStatus, string expected)
    {
        deliveryStatus.GetTagClass().Should().Be(expected);
    }

    [Test]
    public void WhenGettingTagClass_AndUnknownValue_ThenReturnsEmptyString()
    {
        ((DeliveryStatus)999).GetTagClass().Should().BeEmpty();
    }

    [Test]
    public void WhenConvertingToDeliveryStatus_AndNoDate_ThenReturnsOpenToNewStarts()
    {
        ((DateTime?)null).ToDeliveryStatus().Should().Be(DeliveryStatus.OpenToNewStarts);
    }

    [Test]
    public void WhenConvertingToDeliveryStatus_AndFutureDate_ThenReturnsLastStartDateAdded()
    {
        var today = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Unspecified);
        DateTime? futureDate = today.AddDays(1);
        futureDate.ToDeliveryStatus(today).Should().Be(DeliveryStatus.LastStartDateAdded);
    }

    [Test]
    public void WhenConvertingToDeliveryStatus_AndPastDate_ThenReturnsClosedToNewStarts()
    {
        var today = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Unspecified);
        DateTime? pastDate = today.AddDays(-1);
        pastDate.ToDeliveryStatus(today).Should().Be(DeliveryStatus.ClosedToNewStarts);
    }

    [Test]
    public void WhenConvertingToDeliveryStatus_AndIsClosedToNewStarts_ThenReturnsClosedToNewStarts()
    {
        DateTime? futureDate = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Unspecified);
        futureDate.ToDeliveryStatus(isClosedToNewStarts: true).Should().Be(DeliveryStatus.ClosedToNewStarts);
    }

    [Test]
    public void WhenConvertingToDeliveryStatus_AndIsClosedToNewStartsWithNoDate_ThenReturnsClosedToNewStarts()
    {
        ((DateTime?)null).ToDeliveryStatus(isClosedToNewStarts: true).Should().Be(DeliveryStatus.ClosedToNewStarts);
    }

    [Test]
    public void WhenConvertingToDeliveryStatus_AndNotClosedToNewStartsWithNoDate_ThenReturnsOpenToNewStarts()
    {
        ((DateTime?)null).ToDeliveryStatus(isClosedToNewStarts: false).Should().Be(DeliveryStatus.OpenToNewStarts);
    }

    [Test]
    public void WhenConvertingToDeliveryStatus_AndNotClosedToNewStartsWithLastDateStarts_ThenReturnsLastStartDateAdded()
    {
        DateTime? lastDateStarts = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Unspecified);
        lastDateStarts.ToDeliveryStatus(isClosedToNewStarts: false).Should().Be(DeliveryStatus.LastStartDateAdded);
    }
}

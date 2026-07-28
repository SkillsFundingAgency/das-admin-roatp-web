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
        var today = new DateTime(2026, 7, 27);
        DateTime? futureDate = today.AddDays(1);
        futureDate.ToDeliveryStatus(today).Should().Be(DeliveryStatus.LastStartDateAdded);
    }

    [Test]
    public void WhenConvertingToDeliveryStatus_AndPastDate_ThenReturnsClosedToNewStarts()
    {
        var today = new DateTime(2026, 7, 27);
        DateTime? pastDate = today.AddDays(-1);
        pastDate.ToDeliveryStatus(today).Should().Be(DeliveryStatus.ClosedToNewStarts);
    }
}

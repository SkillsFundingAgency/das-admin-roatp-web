using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;

[TestFixture]
public class EnumExtensionsTests
{
    [TestCase(LearningType.Apprenticeship, "Apprenticeship")]
    [TestCase(LearningType.ApprenticeshipUnit, "Apprenticeship unit")]
    [TestCase(LearningType.FoundationApprenticeship, "Foundation apprenticeship")]
    public void GetDescription_ReturnsExpectedDescription(LearningType learningType, string expected)
    {
        learningType.GetDescription().Should().Be(expected);
    }

    [Test]
    public void GetDescription_ReturnsEmptyString_WhenNoDescriptionAttribute()
    {
        TestEnum.ItemWithoutDescription.GetDescription().Should().BeEmpty();
    }

    [TestCase(LearningType.Apprenticeship, "govuk-tag--blue")]
    [TestCase(LearningType.FoundationApprenticeship, "govuk-tag--pink")]
    [TestCase(LearningType.ApprenticeshipUnit, "govuk-tag--purple")]
    public void GetTagClass_ReturnsExpectedClass(LearningType learningType, string expected)
    {
        learningType.GetTagClass().Should().Be(expected);
    }

    [Test]
    public void GetTagClass_ReturnsEmptyString_ForUnknownValue()
    {
        ((LearningType)999).GetTagClass().Should().BeEmpty();
    }

    [TestCase(DeliveryStatus.OpenToNewStarts, "govuk-tag--green")]
    [TestCase(DeliveryStatus.LastStartDateAdded, "govuk-tag--orange")]
    [TestCase(DeliveryStatus.ClosedToNewStarts, "govuk-tag--grey")]
    public void WhenGettingTagClass_AndDeliveryStatusProvided_ThenReturnsExpectedClass(DeliveryStatus deliveryStatus, string expected)
    {
        deliveryStatus.GetTagClass().Should().Be(expected);
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

    private enum TestEnum
    {
        [Description("Described")]
        ItemWithDescription,
        ItemWithoutDescription
    }
}

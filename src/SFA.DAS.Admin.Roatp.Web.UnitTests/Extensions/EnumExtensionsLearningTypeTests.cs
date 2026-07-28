using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;

[TestFixture]
public class EnumExtensionsLearningTypeTests
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

    private enum TestEnum
    {
        [Description("Described")]
        ItemWithDescription,
        ItemWithoutDescription
    }
}

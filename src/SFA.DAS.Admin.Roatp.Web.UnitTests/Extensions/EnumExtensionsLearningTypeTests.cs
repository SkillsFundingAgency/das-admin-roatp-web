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

    [TestCase(LearningType.Apprenticeship, RouteLearningType.Apprenticeship)]
    [TestCase(LearningType.FoundationApprenticeship, RouteLearningType.Apprenticeship)]
    [TestCase(LearningType.ApprenticeshipUnit, RouteLearningType.ApprenticeshipUnit)]
    public void ToRouteLearningType_ReturnsExpectedRouteLearningType(LearningType learningType, RouteLearningType expected)
    {
        learningType.ToRouteLearningType().Should().Be(expected);
    }

    [TestCase(LearningType.Apprenticeship, CourseType.Apprenticeship)]
    [TestCase(LearningType.FoundationApprenticeship, CourseType.Apprenticeship)]
    [TestCase(LearningType.ApprenticeshipUnit, CourseType.ShortCourse)]
    public void ToCourseType_ReturnsExpectedCourseType(LearningType learningType, CourseType expected)
    {
        learningType.ToCourseType().Should().Be(expected);
    }

    [TestCase(RouteLearningType.Apprenticeship, "Apprenticeship")]
    [TestCase(RouteLearningType.ApprenticeshipUnit, "Apprenticeship unit")]
    public void GetDescription_ReturnsExpectedDescription_ForRouteLearningType(RouteLearningType routeLearningType, string expected)
    {
        routeLearningType.GetDescription().Should().Be(expected);
    }

    [TestCase(RouteLearningType.Apprenticeship, CourseType.Apprenticeship)]
    [TestCase(RouteLearningType.ApprenticeshipUnit, CourseType.ShortCourse)]
    public void ToCourseType_ReturnsExpectedCourseType_ForRouteLearningType(RouteLearningType routeLearningType, CourseType expected)
    {
        routeLearningType.ToCourseType().Should().Be(expected);
    }

    private enum TestEnum
    {
        [Description("Described")]
        ItemWithDescription,
        ItemWithoutDescription
    }
}

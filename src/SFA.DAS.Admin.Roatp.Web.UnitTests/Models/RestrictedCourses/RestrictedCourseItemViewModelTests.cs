using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.RestrictedCourses;

[TestFixture]
public class RestrictedCourseItemViewModelTests
{
    [TestCase(LearningType.Apprenticeship, "govuk-tag--blue", "Apprenticeship")]
    [TestCase(LearningType.FoundationApprenticeship, "govuk-tag--pink", "Foundation apprenticeship")]
    [TestCase(LearningType.ApprenticeshipUnit, "govuk-tag--purple", "Apprenticeship unit")]
    public void LearningType_ReturnsExpectedTagClassAndDescription(LearningType learningType, string expectedClass, string expectedDescription)
    {
        var model = new RestrictedCourseItemViewModel { LearningType = learningType };

        model.LearningTypeTagClass.Should().Be(expectedClass);
        model.LearningTypeDescription.Should().Be(expectedDescription);
    }

    [Test]
    public void DisplayTitle_IncludesLevel()
    {
        var model = new RestrictedCourseItemViewModel
        {
            Title = "Bricklaying",
            Level = 2
        };

        model.DisplayTitle.Should().Be("Bricklaying (Level 2)");
    }

    [Test]
    public void ImplicitConversion_MapsRestrictedCourseModel()
    {
        var course = new RestrictedCourseModel
        {
            LarsCode = "124",
            Title = "Bricklaying",
            Level = 2,
            LearningType = LearningType.ApprenticeshipUnit
        };

        RestrictedCourseItemViewModel model = course;

        model.LarsCode.Should().Be("124");
        model.Title.Should().Be("Bricklaying");
        model.Level.Should().Be(2);
        model.LearningType.Should().Be(LearningType.ApprenticeshipUnit);
    }
}

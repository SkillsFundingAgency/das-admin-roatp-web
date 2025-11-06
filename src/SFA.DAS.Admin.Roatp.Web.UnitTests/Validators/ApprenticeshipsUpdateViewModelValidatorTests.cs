using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;
public class ApprenticeshipsUpdateViewModelValidatorTests
{
    private ApprenticeshipsUpdateViewModelValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new ApprenticeshipsUpdateViewModelValidator();
    }

    [Test]
    public void TestValidator_NoApprenticeshipsAndNoApprenticeshipUnits_Invalid_ReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new ApprenticeshipsUpdateViewModel { ApprenticeshipsSelectionChoice = null });

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.ApprenticeshipsSelectionChoice)
            .WithErrorMessage(ApprenticeshipsUpdateViewModelValidator.ApprenticeshipsSelectionErrorMessage);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestValidator_ValidCourseTypes_ReturnsValid(bool offersApprenticeshipUnits)
    {
        var model = new ApprenticeshipsUpdateViewModel { ApprenticeshipsSelectionChoice = offersApprenticeshipUnits };
        var result = _validator.TestValidate(model);

        result.IsValid.Should().BeTrue();
    }
}

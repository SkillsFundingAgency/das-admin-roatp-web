using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;
public class ApprenticeshipUnitUpdateViewModelValidatorTests
{
    private ApprenticeshipUnitUpdateViewModelValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new ApprenticeshipUnitUpdateViewModelValidator();
    }

    [TestCase(false, false)]
    public void TestValidator_NoApprenticeshipsAndNoApprenticeshipUnits_Invalid_ReturnsExpectedErrorMessage(bool offersApprenticeships, bool offersApprenticeshipUnits)
    {
        var result = _validator.TestValidate(new ApprenticeshipUnitsUpdateViewModel { OffersApprenticeships = offersApprenticeships, ApprenticeshipUnitsSelectionId = offersApprenticeshipUnits });

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.ApprenticeshipUnitsSelectionId)
            .WithErrorMessage(ApprenticeshipUnitUpdateViewModelValidator.NoSelectionsMadeErrorMessage);
    }

    [TestCase(false, true)]
    [TestCase(true, true)]
    [TestCase(true, false)]
    public void TestValidator_ValidCourseTypes_ReturnsValid(bool offersApprentices, bool offersApprenticeshipUnits)
    {
        var model = new ApprenticeshipUnitsUpdateViewModel { OffersApprenticeships = offersApprentices, ApprenticeshipUnitsSelectionId = offersApprenticeshipUnits };
        var result = _validator.TestValidate(model);

        result.IsValid.Should().BeTrue();
    }
}

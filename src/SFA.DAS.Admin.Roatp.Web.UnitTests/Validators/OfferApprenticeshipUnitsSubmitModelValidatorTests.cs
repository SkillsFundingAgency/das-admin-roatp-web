using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;
public class OfferApprenticeshipUnitsSubmitModelValidatorTests
{
    private OfferApprenticeshipUnitsSubmitModelValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new OfferApprenticeshipUnitsSubmitModelValidator();
    }

    [Test]
    public void TestValidator_ApprenticeshipUnitsSelectionId_Invalid_ReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new OfferApprenticeshipUnitsSubmitModel());

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.ApprenticeshipUnitsSelectionId)
            .WithErrorMessage(OfferApprenticeshipUnitsSubmitModelValidator.ApprenticeshipUnitSelectionErrorMessage);
    }

    [Test]
    public void TestValidator_ApprenticeshipUnitsSelectionId_Valid_ReturnsValid()
    {
        bool apprenticeshipUnitsSelection = true;
        var result = _validator.TestValidate(new OfferApprenticeshipUnitsSubmitModel { ApprenticeshipUnitsSelectionId = apprenticeshipUnitsSelection });
        result.IsValid.Should().BeTrue();
    }
}

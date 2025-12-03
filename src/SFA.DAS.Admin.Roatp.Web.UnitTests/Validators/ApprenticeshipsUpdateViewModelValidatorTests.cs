using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;
public class ApprenticeshipsUpdateViewModelValidatorTests
{
    private OfferApprenticeshipsSubmitModelValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new OfferApprenticeshipsSubmitModelValidator();
    }

    [Test]
    public void TestValidator_NoApprenticeshipsAndNoApprenticeshipUnits_Invalid_ReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new OfferApprenticeshipsViewModel { IsApprenticeshipsOffered = null });

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.IsApprenticeshipsOffered)
            .WithErrorMessage(OfferApprenticeshipsSubmitModelValidator.ApprenticeshipsSelectionErrorMessage);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestValidator_ValidCourseTypes_ReturnsValid(bool offersApprenticeshipUnits)
    {
        var model = new OfferApprenticeshipsViewModel { IsApprenticeshipsOffered = offersApprenticeshipUnits };
        var result = _validator.TestValidate(model);

        result.IsValid.Should().BeTrue();
    }
}

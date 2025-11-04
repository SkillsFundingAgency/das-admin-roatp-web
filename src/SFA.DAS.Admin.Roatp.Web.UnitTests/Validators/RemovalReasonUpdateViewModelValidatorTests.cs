using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

public class RemovalReasonUpdateViewModelValidatorTests
{
    private RemovalReasonUpdateViewModelValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new RemovalReasonUpdateViewModelValidator();
    }

    [Test]
    public void TestValidator_RemovalReasonId_Invalid_ReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new RemovalReasonUpdateViewModel());

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.RemovalReasonId)
            .WithErrorMessage(RemovalReasonUpdateViewModelValidator.NoRemovalReasonSelectedErrorMessage);
    }

    [Test]
    public void TestValidator_RemovalReasonId_Valid_ReturnsValid()
    {
        int removalReasonId = 1;
        var result = _validator.TestValidate(new RemovalReasonUpdateViewModel { RemovalReasonId = removalReasonId });
        result.IsValid.Should().BeTrue();
    }
}

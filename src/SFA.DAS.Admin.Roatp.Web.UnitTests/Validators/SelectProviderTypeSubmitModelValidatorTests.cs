using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;
public class SelectProviderTypeSubmitModelValidatorTests
{
    private SelectProviderTypeSubmitModelValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new SelectProviderTypeSubmitModelValidator();
    }

    [Test]
    public void TestValidator_SelectedProviderTypeId_Invalid_ReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new SelectProviderTypeSubmitModel());

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.SelectedProviderTypeId)
            .WithErrorMessage(SelectProviderTypeSubmitModelValidator.NoProviderTypeSelectedErrorMessage);
    }

    [Test]
    public void TestValidator_SelectedProviderTypeId_Valid_ReturnsValid()
    {
        int providerTypeId = 1;
        var result = _validator.TestValidate(new SelectProviderTypeSubmitModel { SelectedProviderTypeId = providerTypeId });
        result.IsValid.Should().BeTrue();
    }
}
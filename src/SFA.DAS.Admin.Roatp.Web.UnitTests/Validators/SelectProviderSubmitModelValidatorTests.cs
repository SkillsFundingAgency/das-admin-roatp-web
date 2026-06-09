using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

public class SelectProviderSubmitModelValidatorTests
{
    [TestCase("")]
    [TestCase(null)]
    [TestCase("1234567")]
    [TestCase("123456789")]
    [TestCase("23456789")]
    [TestCase("abcdefgh")]
    public void Validate_InvalidUkprn_FailsValidation(string? ukprn)
    {
        SelectProviderSubmitModelValidator sut = new();

        var result = sut.TestValidate(new SelectProviderSubmitModel() { Ukprn = ukprn });

        result.ShouldHaveValidationErrorFor(m => m.Ukprn).WithErrorMessage(UkprnValidator.UkprnFormatValidationMessage);
    }

    [Test]
    public void Validate_ValidUkprn_PassesValidation()
    {
        string ukprn = "12345678";
        SelectProviderSubmitModelValidator sut = new();

        var result = sut.TestValidate(new SelectProviderSubmitModel() { Ukprn = ukprn });

        result.ShouldNotHaveValidationErrorFor(m => m.Ukprn);
    }
}

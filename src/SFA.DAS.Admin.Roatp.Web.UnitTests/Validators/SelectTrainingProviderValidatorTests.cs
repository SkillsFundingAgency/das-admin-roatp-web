using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;
public class SelectTrainingProviderValidatorTests
{
    private SelectTrainingProviderValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new SelectTrainingProviderValidator();
    }

    [Test]
    public void TestValidator_SearchTerm_Invalid_ReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new SelectTrainingProviderSubmitViewModel());

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.SearchTerm)
            .WithErrorMessage(SelectTrainingProviderValidator.NoTrainingProviderSelectedErrorMessage);
    }

    [Test]
    public void TestValidator_SearchTerm_Valid_ReturnsValid()
    {
        var ukprn = "10001000";
        var result = _validator.TestValidate(new SelectTrainingProviderSubmitViewModel { Ukprn = ukprn });
        result.IsValid.Should().BeTrue();
    }
}

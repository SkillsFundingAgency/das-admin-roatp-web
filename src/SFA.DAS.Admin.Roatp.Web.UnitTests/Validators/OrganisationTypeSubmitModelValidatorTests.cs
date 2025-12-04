using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;
public class OrganisationTypeSubmitModelValidatorTests
{
    private OrganisationTypeSubmitModelValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new OrganisationTypeSubmitModelValidator();
    }

    [Test]
    public void TestValidator_OrganisationTypeId_Invalid_ReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new OrganisationTypeSubmitModel());

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.OrganisationTypeId)
            .WithErrorMessage(OrganisationTypeSubmitModelValidator.OrganisationTypeSelectionErrorMessage);
    }

    [Test]
    public void TestValidator_OrganisationTypeId_Valid_ReturnsValid()
    {
        int organisationTypeId = 1;
        var result = _validator.TestValidate(new OrganisationTypeSubmitModel { OrganisationTypeId = organisationTypeId });
        result.IsValid.Should().BeTrue();
    }
}

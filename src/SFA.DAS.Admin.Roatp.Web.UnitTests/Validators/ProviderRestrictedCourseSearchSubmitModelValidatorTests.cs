using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

public class ProviderRestrictedCourseSearchSubmitModelValidatorTests
{
    private ProviderRestrictedCourseSearchSubmitModelValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new ProviderRestrictedCourseSearchSubmitModelValidator();
    }

    [Test]
    public void WhenValidatingSelectedLarsCode_AndNoCourseSelected_ThenReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new ProviderRestrictedCourseSearchSubmitModel());

        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(c => c.SelectedLarsCode)
                .WithErrorMessage(ProviderRestrictedCourseSearchSubmitModelValidator.NoCourseSelectedErrorMessage);
        }
    }

    [Test]
    public void WhenValidatingSelectedLarsCode_AndCourseIsSelected_ThenIsValid()
    {
        var result = _validator.TestValidate(new ProviderRestrictedCourseSearchSubmitModel
        {
            SelectedLarsCode = "123"
        });

        result.IsValid.Should().BeTrue();
    }
}

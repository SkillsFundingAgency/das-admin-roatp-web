using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class ChangeCourseRestrictionSubmitModelValidatorTests
{
    private ChangeCourseRestrictionSubmitModelValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ChangeCourseRestrictionSubmitModelValidator();
    }

    [Test]
    public void WhenNoOptionSelected_ThenReturnsError()
    {
        var result = _sut.TestValidate(new ChangeCourseRestrictionSubmitModel());

        result.ShouldHaveValidationErrorFor(model => model.SelectedOption)
            .WithErrorMessage(ChangeCourseRestrictionSubmitModelValidator.NoOptionSelectedErrorMessage);
    }

    [Test]
    public void WhenInvalidOptionSelected_ThenReturnsError()
    {
        var result = _sut.TestValidate(new ChangeCourseRestrictionSubmitModel { SelectedOption = "Invalid" });

        result.ShouldHaveValidationErrorFor(model => model.SelectedOption)
            .WithErrorMessage(ChangeCourseRestrictionSubmitModelValidator.NoOptionSelectedErrorMessage);
    }

    [TestCase("Change")]
    [TestCase("Remove")]
    public void WhenValidOptionSelected_ThenPasses(string selectedOption)
    {
        var result = _sut.TestValidate(new ChangeCourseRestrictionSubmitModel { SelectedOption = selectedOption });

        result.ShouldNotHaveAnyValidationErrors();
    }
}

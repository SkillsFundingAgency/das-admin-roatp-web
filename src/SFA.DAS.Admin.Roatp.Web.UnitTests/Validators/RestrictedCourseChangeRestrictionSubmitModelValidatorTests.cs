using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class RestrictedCourseChangeRestrictionSubmitModelValidatorTests
{
    private RestrictedCourseChangeRestrictionSubmitModelValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new RestrictedCourseChangeRestrictionSubmitModelValidator();
    }

    [Test]
    public void WhenNoOptionSelected_ThenReturnsError()
    {
        var result = _sut.TestValidate(new RestrictedCourseChangeRestrictionSubmitModel());

        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(model => model.SelectedOption)
                .WithErrorMessage(RestrictedCourseChangeRestrictionSubmitModelValidator.NoOptionSelectedErrorMessage);
        }
    }

    [Test]
    public void WhenInvalidOptionSelected_ThenReturnsError()
    {
        var result = _sut.TestValidate(new RestrictedCourseChangeRestrictionSubmitModel { SelectedOption = "Invalid" });

        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(model => model.SelectedOption)
                .WithErrorMessage(RestrictedCourseChangeRestrictionSubmitModelValidator.NoOptionSelectedErrorMessage);
        }
    }

    [TestCase("AddOrChange")]
    [TestCase("Remove")]
    public void WhenValidOptionSelected_ThenPasses(string selectedOption)
    {
        var result = _sut.TestValidate(new RestrictedCourseChangeRestrictionSubmitModel { SelectedOption = selectedOption });

        result.IsValid.Should().BeTrue();
    }
}

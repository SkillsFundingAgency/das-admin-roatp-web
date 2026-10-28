using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class ChangeAllowedCourseLastStartDateSubmitModelValidatorTests
{
    private ChangeAllowedCourseLastStartDateSubmitModelValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ChangeAllowedCourseLastStartDateSubmitModelValidator();
    }

    [Test]
    public void WhenNoOptionSelected_ThenReturnsError()
    {
        var result = _sut.TestValidate(new ChangeAllowedCourseLastStartDateSubmitModel());

        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(model => model.SelectedOption)
                .WithErrorMessage(ChangeAllowedCourseLastStartDateSubmitModelValidator.NoOptionSelectedErrorMessage);
        }
    }

    [TestCase("Change")]
    [TestCase("Remove")]
    public void WhenValidOptionSelected_ThenPasses(string selectedOption)
    {
        var result = _sut.TestValidate(new ChangeAllowedCourseLastStartDateSubmitModel { SelectedOption = selectedOption });

        result.IsValid.Should().BeTrue();
    }
}

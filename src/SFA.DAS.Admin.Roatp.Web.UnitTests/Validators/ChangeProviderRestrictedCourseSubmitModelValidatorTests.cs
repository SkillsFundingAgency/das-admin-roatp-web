using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class ChangeProviderRestrictedCourseSubmitModelValidatorTests
{
    private ChangeProviderRestrictedCourseSubmitModelValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ChangeProviderRestrictedCourseSubmitModelValidator();
    }

    [Test]
    public void WhenNoOptionSelected_ThenReturnsError()
    {
        var result = _sut.TestValidate(new ChangeProviderRestrictedCourseSubmitModel());

        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(model => model.SelectedOption)
                .WithErrorMessage(ChangeProviderRestrictedCourseSubmitModelValidator.NoOptionSelectedErrorMessage);
        }
    }

    [Test]
    public void WhenInvalidOptionSelected_ThenReturnsError()
    {
        var result = _sut.TestValidate(new ChangeProviderRestrictedCourseSubmitModel { SelectedOption = "Invalid" });

        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(model => model.SelectedOption)
                .WithErrorMessage(ChangeProviderRestrictedCourseSubmitModelValidator.NoOptionSelectedErrorMessage);
        }
    }

    [TestCase("AddOrChange")]
    [TestCase("Remove")]
    public void WhenValidOptionSelected_ThenPasses(string selectedOption)
    {
        var result = _sut.TestValidate(new ChangeProviderRestrictedCourseSubmitModel { SelectedOption = selectedOption });

        result.IsValid.Should().BeTrue();
    }
}

using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class ChangeLastDateStartsSubmitModelValidatorTests
{
    private ChangeLastDateStartsSubmitModelValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ChangeLastDateStartsSubmitModelValidator();
    }

    [Test]
    public void WhenNoOptionSelected_ThenReturnsError()
    {
        var result = _sut.TestValidate(new ChangeLastDateStartsSubmitModel());

        result.ShouldHaveValidationErrorFor(model => model.SelectedOption)
            .WithErrorMessage(ChangeLastDateStartsSubmitModelValidator.NoOptionSelectedErrorMessage);
    }

    [Test]
    public void WhenInvalidOptionSelected_ThenReturnsError()
    {
        var result = _sut.TestValidate(new ChangeLastDateStartsSubmitModel { SelectedOption = "Invalid" });

        result.ShouldHaveValidationErrorFor(model => model.SelectedOption)
            .WithErrorMessage(ChangeLastDateStartsSubmitModelValidator.NoOptionSelectedErrorMessage);
    }

    [TestCase(ChangeLastDateStartsOptions.Change)]
    [TestCase(ChangeLastDateStartsOptions.Remove)]
    public void WhenValidOptionSelected_ThenPasses(string selectedOption)
    {
        var result = _sut.TestValidate(new ChangeLastDateStartsSubmitModel { SelectedOption = selectedOption });

        result.ShouldNotHaveAnyValidationErrors();
    }
}

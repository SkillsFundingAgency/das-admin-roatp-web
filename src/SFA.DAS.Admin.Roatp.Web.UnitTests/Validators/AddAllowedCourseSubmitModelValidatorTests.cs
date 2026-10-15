using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

public class AddAllowedCourseSubmitModelValidatorTests
{
    private AddAllowedCourseSubmitModelValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new AddAllowedCourseSubmitModelValidator();
    }

    [Test]
    public void WhenValidatingSelectedLarsCode_AndNoCourseSelected_ThenReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new AddAllowedCourseSubmitModel());

        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(c => c.SelectedLarsCode)
                .WithErrorMessage(AddAllowedCourseSubmitModelValidator.NoCourseSelectedErrorMessage);
        }
    }

    [Test]
    public void WhenValidatingSelectedLarsCode_AndCourseIsSelected_ThenIsValid()
    {
        var result = _validator.TestValidate(new AddAllowedCourseSubmitModel
        {
            SelectedLarsCode = "105"
        });

        result.IsValid.Should().BeTrue();
    }
}

using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

public class UnrestrictedCourseSearchSubmitModelValidatorTests
{
    private UnrestrictedCourseSearchSubmitModelValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new UnrestrictedCourseSearchSubmitModelValidator();
    }

    [Test]
    public void WhenValidatingSelectedLarsCode_AndNoCourseSelected_ThenReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new UnrestrictedCourseSearchSubmitModel());

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.SelectedLarsCode)
            .WithErrorMessage(UnrestrictedCourseSearchSubmitModelValidator.NoCourseSelectedErrorMessage);
    }

    [Test]
    public void WhenValidatingSelectedLarsCode_AndCourseIsSelected_ThenIsValid()
    {
        var result = _validator.TestValidate(new UnrestrictedCourseSearchSubmitModel
        {
            SelectedLarsCode = "123"
        });

        result.IsValid.Should().BeTrue();
    }
}

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
    public void WhenValidatingLarsCode_AndNoCourseSelected_ThenReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new UnrestrictedCourseSearchSubmitModel());

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.SearchTerm)
            .WithErrorMessage(UnrestrictedCourseSearchSubmitModelValidator.NoCourseSelectedErrorMessage);
    }

    [Test]
    public void WhenValidatingLarsCode_AndCourseIsSelected_ThenIsValid()
    {
        var result = _validator.TestValidate(new UnrestrictedCourseSearchSubmitModel
        {
            LarsCode = "123",
            Title = "Software developer",
            Level = 4
        });

        result.IsValid.Should().BeTrue();
    }
}

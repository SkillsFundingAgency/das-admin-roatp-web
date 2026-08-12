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
    public void WhenValidatingSearchTerm_AndNoCourseSelected_ThenReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new UnrestrictedCourseSearchSubmitModel());

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.SearchTerm)
            .WithErrorMessage(UnrestrictedCourseSearchSubmitModelValidator.NoCourseSelectedErrorMessage);
    }

    [Test]
    public void WhenValidatingSearchTerm_AndCourseIsSelected_ThenIsValid()
    {
        var result = _validator.TestValidate(new UnrestrictedCourseSearchSubmitModel
        {
            Title = "Software developer",
            Level = 4,
            LarsCode = "123"
        });

        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void WhenValidatingSearchTerm_AndLarsCodeMissing_ThenReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new UnrestrictedCourseSearchSubmitModel
        {
            Title = "Software developer",
            Level = 4
        });

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.LarsCode)
            .WithErrorMessage(UnrestrictedCourseSearchSubmitModelValidator.NoCourseSelectedErrorMessage);
    }
}

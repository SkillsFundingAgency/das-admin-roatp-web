using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

public class UnrestrictedCourseSearchValidatorTests
{
    private UnrestrictedCourseSearchValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new UnrestrictedCourseSearchValidator();
    }

    [Test]
    public void WhenValidatingSearchTerm_AndNoCourseSelected_ThenReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new UnrestrictedCourseSearchViewModel());

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.SearchTerm)
            .WithErrorMessage(UnrestrictedCourseSearchValidator.NoCourseSelectedErrorMessage);
    }

    [Test]
    public void WhenValidatingSearchTerm_AndCourseIsSelected_ThenIsValid()
    {
        var result = _validator.TestValidate(new UnrestrictedCourseSearchViewModel
        {
            LarsCode = "123",
            Title = "Software developer",
            Level = 4
        });

        result.IsValid.Should().BeTrue();
    }
}

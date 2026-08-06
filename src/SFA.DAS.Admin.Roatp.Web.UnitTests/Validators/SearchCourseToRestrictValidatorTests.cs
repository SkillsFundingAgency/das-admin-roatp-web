using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

public class SearchCourseToRestrictValidatorTests
{
    private SearchCourseToRestrictValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new SearchCourseToRestrictValidator();
    }

    [Test]
    public void WhenValidatingSearchTerm_AndNoCourseSelected_ThenReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new SearchCourseToRestrictViewModel());

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(c => c.SearchTerm)
            .WithErrorMessage(SearchCourseToRestrictValidator.NoCourseSelectedErrorMessage);
    }

    [Test]
    public void WhenValidatingSearchTerm_AndCourseIsSelected_ThenIsValid()
    {
        var result = _validator.TestValidate(new SearchCourseToRestrictViewModel
        {
            LarsCode = "123",
            Title = "Software developer",
            Level = 4
        });

        result.IsValid.Should().BeTrue();
    }
}

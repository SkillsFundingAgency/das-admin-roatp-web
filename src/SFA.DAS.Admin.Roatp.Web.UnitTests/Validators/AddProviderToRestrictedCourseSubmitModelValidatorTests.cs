using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class AddProviderToRestrictedCourseSubmitModelValidatorTests
{
    private AddProviderToRestrictedCourseSubmitModelValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new AddProviderToRestrictedCourseSubmitModelValidator();
    }

    [Test]
    public void WhenNoProviderSelected_ThenReturnsError()
    {
        var result = _sut.TestValidate(new AddProviderToRestrictedCourseSubmitModel());

        result.ShouldHaveValidationErrorFor(model => model.SelectedUkprn)
            .WithErrorMessage(AddProviderToRestrictedCourseSubmitModelValidator.NoProviderSelectedErrorMessage);
    }

    [Test]
    public void WhenProviderSelected_ThenPasses()
    {
        var result = _sut.TestValidate(new AddProviderToRestrictedCourseSubmitModel
        {
            SelectedUkprn = "10007938"
        });

        result.ShouldNotHaveAnyValidationErrors();
    }
}

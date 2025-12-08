using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;
public class ProviderDetailsSummaryViewModelValidatiorTests
{
    private ProviderDetailsSummaryViewModelValidatior _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new ProviderDetailsSummaryViewModelValidatior();
    }

    [Test]
    public void TestValidator_IsNotSupprotingProviderAndHasNotSelectedACourseType_Invalid_ReturnsExpectedErrorMessage()
    {
        var result = _validator.TestValidate(new ProviderDetailsSummaryViewModel() { IsSupportingProvider = false, OffersApprenticeshipsText = "No", OffersApprenticeshipUnitsText = "No" });

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(s => s.IsSupportingProvider)
            .WithErrorMessage(ProviderDetailsSummaryViewModelValidatior.RequiredCourseTypeSelectionErrorMessage);
    }

    [Test]
    public void TestValidator_IsNotSupprotingProviderAndHasSelectedACourseType_Valid_ReturnsValid()
    {
        var result = _validator.TestValidate(new ProviderDetailsSummaryViewModel() { IsSupportingProvider = false, OffersApprenticeshipsText = "Yes", OffersApprenticeshipUnitsText = "No" });

        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveValidationErrorFor(s => s.IsSupportingProvider);
    }

    [Test]
    public void TestValidator_IsSupprotingProvider_Valid_ReturnsValid()
    {
        var result = _validator.TestValidate(new ProviderDetailsSummaryViewModel() { IsSupportingProvider = true });
        result.IsValid.Should().BeTrue();
    }
}

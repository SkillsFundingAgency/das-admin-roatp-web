using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;
public class ProviderDetailsSummaryViewModelValidatiorTests
{
    private ProviderDetailsSummaryViewModelValidatior _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new ProviderDetailsSummaryViewModelValidatior();
    }

    [Test, MoqAutoData]
    public void TestValidator_IsNotSupprotingProviderAndHasNotSelectedACourseType_Invalid_ReturnsExpectedErrorMessage(ProviderDetailsSummaryViewModel viewModel)
    {
        viewModel.IsSupportingProvider = false;
        viewModel.OffersApprenticeshipsText = "No";
        viewModel.OffersApprenticeshipUnitsText = "No";

        var result = _validator.TestValidate(viewModel);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(s => s)
            .WithErrorMessage(ProviderDetailsSummaryViewModelValidatior.RequiredCourseTypeSelectionErrorMessage);
    }

    [Test, MoqAutoData]
    public void TestValidator_IsNotSupprotingProviderAndHasSelectedACourseType_Valid_ReturnsValid(ProviderDetailsSummaryViewModel viewModel)
    {
        viewModel.IsSupportingProvider = false;
        viewModel.OffersApprenticeshipsText = "Yes";
        viewModel.OffersApprenticeshipUnitsText = "No";

        var result = _validator.TestValidate(viewModel);

        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveValidationErrorFor(s => s);
    }

    [Test, MoqAutoData]
    public void TestValidator_IsSupprotingProvider_Valid_ReturnsValid(ProviderDetailsSummaryViewModel viewModel)
    {
        viewModel.IsSupportingProvider = true;

        var result = _validator.TestValidate(viewModel);
        result.IsValid.Should().BeTrue();
    }

    [Test, MoqAutoData]
    public void TestValidator_IsNotSupprotingProviderAndCourseTypeIsMissing_Invalid_ReturnsExpectedErrorMessage(ProviderDetailsSummaryViewModel viewModel)
    {
        viewModel.IsSupportingProvider = false;
        viewModel.OffersApprenticeshipsText = null;
        viewModel.OffersApprenticeshipUnitsText = null;

        var result = _validator.TestValidate(viewModel);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(s => s)
            .WithErrorMessage(ProviderDetailsSummaryViewModelValidatior.RequiredFieldsNotSetErrorMessage);
    }

    [Test, MoqAutoData]
    public void TestValidator_IsSupprotingProviderAndCourseTypeIsMissing_Valid_ReturnsValid(ProviderDetailsSummaryViewModel viewModel)
    {
        viewModel.IsSupportingProvider = true;
        viewModel.OffersApprenticeshipsText = null;
        viewModel.OffersApprenticeshipUnitsText = null;

        var result = _validator.TestValidate(viewModel);
        result.IsValid.Should().BeTrue();
    }

    [Test, MoqAutoData]
    public void TestValidator_ProviderRouteIsMissing_Invalid_ReturnsExpectedErrorMessage(ProviderDetailsSummaryViewModel viewModel)
    {
        viewModel.ProviderRoute = null;

        var result = _validator.TestValidate(viewModel);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(s => s.ProviderRoute)
            .WithErrorMessage(ProviderDetailsSummaryViewModelValidatior.RequiredFieldsNotSetErrorMessage);
    }

    [Test, MoqAutoData]
    public void TestValidator_OrganisationTypeIsMissing_Invalid_ReturnsExpectedErrorMessage(ProviderDetailsSummaryViewModel viewModel)
    {
        viewModel.OrganisationType = null;

        var result = _validator.TestValidate(viewModel);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(s => s.OrganisationType)
            .WithErrorMessage(ProviderDetailsSummaryViewModelValidatior.RequiredFieldsNotSetErrorMessage);
    }

    [Test, MoqAutoData]
    public void TestValidator_AllRequiredDataPopulated_Valid_ReturnsValid(ProviderDetailsSummaryViewModel viewModel)
    {
        var result = _validator.TestValidate(viewModel);
        result.IsValid.Should().BeTrue();
    }
}

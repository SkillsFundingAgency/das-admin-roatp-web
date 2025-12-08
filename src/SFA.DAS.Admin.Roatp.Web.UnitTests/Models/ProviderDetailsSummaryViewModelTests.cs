using AutoFixture.NUnit3;
using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models;
public class ProviderDetailsSummaryViewModelTests
{
    [Test]
    [InlineAutoData(1, true, true, "Yes", "Yes", false)]
    [InlineAutoData(1, false, false, "No", "No", false)]
    [InlineAutoData(1, false, true, "No", "Yes", false)]
    [InlineAutoData(1, true, false, "Yes", "No", false)]
    [InlineAutoData(2, true, true, "Yes", "Yes", false)]
    [InlineAutoData(3, null, null, "No", "No", true)]
    public void ProviderDetailsSummaryViewModel_ImplicitOperator_MapsPropertiesCorrectly(
        int providerTypeId,
        bool? offersApprenticeships,
        bool? offersApprenticeshipUnits,
        string expectedOffersApprenticeshipsText,
        string expectedOffersApprenticeshipUnitsText,
        bool expectedIsSupportingProvider,
        AddProviderSessionModel sessionModel)
    {
        // Arrange
        sessionModel.ProviderTypeId = providerTypeId;
        sessionModel.OffersApprenticeships = offersApprenticeships;
        sessionModel.OffersApprenticeshipUnits = offersApprenticeshipUnits;

        // Act
        ProviderDetailsSummaryViewModel sut = sessionModel;

        // Assert
        sut.Ukprn.Should().Be(sessionModel.Ukprn);
        sut.LegalName.Should().Be(sessionModel.LegalName);
        sut.TradingName.Should().Be(sessionModel.TradingName);
        sut.ProviderRoute.Should().Be(((ProviderType)sessionModel.ProviderTypeId!).ToString());
        sut.OffersApprenticeshipsText.Should().Be(expectedOffersApprenticeshipsText);
        sut.OffersApprenticeshipUnitsText.Should().Be(expectedOffersApprenticeshipUnitsText);
        sut.OrganisationType.Should().Be(sessionModel.OrganisationType);
        sut.IsSupportingProvider.Should().Be(expectedIsSupportingProvider);
    }
}

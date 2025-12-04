using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.SelectOfferApprenticeshipsControllerTests;
public class SelectOfferApprenticeshipsControllerGetTests
{
    [Test]
    [MoqInlineAutoData(null)]
    [MoqInlineAutoData(false)]
    [MoqInlineAutoData(true)]
    public void Get_Index_SessionsReturnsVariationOfOfferApprenticeshipsInput_ReturnsViewModel(
        bool? offerApprenticeships,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectOfferApprenticeshipsController sut)
    {
        // Arrange
        var sessionModel = new AddProviderSessionModel()
        {
            Ukprn = 12345678,
            LegalName = "LegalName",
            TradingName = "TradingName",
            CompanyNumber = "12345678",
            CharityNumber = "12345678",
            ProviderTypeId = 1,
            OffersApprenticeships = offerApprenticeships,
        };

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        var expectedApprenticeshipTypesChoices = BuildApprenticeshipsChoices(offerApprenticeships);

        // Act
        var result = sut.Index() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        result!.Model.Should().BeOfType<OfferApprenticeshipsViewModel>();
        var model = result!.Model as OfferApprenticeshipsViewModel;
        model!.ApprenticeshipsSelection.Should().BeEquivalentTo(expectedApprenticeshipTypesChoices);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
    }

    [Test, MoqAutoData]
    public void Get_Index_SessionReturnSupportingProvider_RedirectsToHome(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectOfferApprenticeshipsController sut)
    {
        // Arrange
        var sessionModel = new AddProviderSessionModel()
        {
            LegalName = "LegalName",
            ProviderTypeId = 3
        };

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);


        // Act
        var result = sut.Index();

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result! as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult!.RouteName.Should().Be(RouteNames.Home);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
    }

    private static List<ApprenticeshipsSelectionModel> BuildApprenticeshipsChoices(bool? containsApprenticeshipUnits)
    {
        return new List<ApprenticeshipsSelectionModel>
            {
                new() { Description = "Yes", Id = true, IsSelected = containsApprenticeshipUnits is true},
                new() { Description = "No", Id = false, IsSelected =  containsApprenticeshipUnits is false}
            };
    }
}

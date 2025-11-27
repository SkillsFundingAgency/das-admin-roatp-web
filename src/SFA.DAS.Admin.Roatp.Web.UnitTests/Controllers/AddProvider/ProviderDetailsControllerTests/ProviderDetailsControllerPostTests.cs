using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.ProviderDetailsControllerTests;
public class ProviderDetailsControllerPostTests
{
    [Test, MoqAutoData]
    public void Post_Index_ProviderTypeIdInSessionIsNull_RedirectsToCorrectAction(
        [Frozen] Mock<ISessionService> sessionServiceMock)
    {
        // Arrange
        var sessionModel = new AddProviderSessionModel()
        {
            Ukprn = 12345678,
            LegalName = "LegalName",
            TradingName = "TradingName",
            CompanyNumber = "12345678",
            CharityNumber = "12345678",
            ProviderTypeId = null
        };

        var model = new ProviderDetailsViewModel()
        {
            Ukprn = sessionModel.Ukprn,
            LegalName = sessionModel.LegalName,
            TradingName = sessionModel.TradingName,
            CompanyNumber = sessionModel.CompanyNumber,
            CharityNumber = sessionModel.CharityNumber,
            SelectProviderUrl = Guid.NewGuid().ToString()
        };

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        var sut = new ProviderDetailsController(sessionServiceMock.Object);

        // Act
        var result = sut.Index(model);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result! as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult!.RouteName.Should().Be(RouteNames.SelectProviderType);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, sessionModel), Times.Never);
    }

    [Test, MoqAutoData]
    public void Post_Index_ProviderTypeIdHasValue_UpdatesSessionAndRedirectsToCorrectAction(
        [Frozen] Mock<ISessionService> sessionServiceMock)
    {
        // Arrange
        var sessionModel = new AddProviderSessionModel()
        {
            Ukprn = 12345678,
            LegalName = "LegalName",
            TradingName = "TradingName",
            CompanyNumber = "12345678",
            CharityNumber = "12345678",
            ProviderTypeId = 1
        };

        var model = new ProviderDetailsViewModel()
        {
            Ukprn = sessionModel.Ukprn,
            LegalName = sessionModel.LegalName,
            TradingName = sessionModel.TradingName,
            CompanyNumber = sessionModel.CompanyNumber,
            CharityNumber = sessionModel.CharityNumber,
            SelectProviderUrl = Guid.NewGuid().ToString()
        };

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        var sut = new ProviderDetailsController(sessionServiceMock.Object);

        // Act
        var result = sut.Index(model);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result! as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult!.RouteName.Should().Be(RouteNames.SelectProviderType);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, sessionModel), Times.Once);
    }
}

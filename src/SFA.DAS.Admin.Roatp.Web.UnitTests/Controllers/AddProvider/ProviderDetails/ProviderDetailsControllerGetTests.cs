using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.ProviderDetails;
public class ProviderDetailsControllerGetTests
{
    [Test, MoqAutoData]
    public void Get_Index_SessionIsNull_RedirectsToAddProvider(
        [Frozen] Mock<ISessionService> sessionServiceMock)
    {
        // Arrange
        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(() => null!);

        var sut = new ProviderDetailsController(sessionServiceMock.Object);

        // Act
        var result = sut.Index();

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result! as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult!.RouteName.Should().Be(RouteNames.AddProvider);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
    }

    [Test, MoqAutoData]
    public void Get_Index_SessionReturnsData_ReturnsViewWithModel(
        [Frozen] Mock<ISessionService> sessionServiceMock)
    {
        // Arrange
        var sessionModel = new AddProviderSessionModel()
        {
            Ukprn = 12345678,
            LegalName = "LegalName",
            TradingName = "TradingName",
            CompanyNumber = "12345678",
            CharityNumber = "12345678"
        };

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        var sut = new ProviderDetailsController(sessionServiceMock.Object);
        var addProvideLink = Guid.NewGuid().ToString();
        var providerDetailsLink = Guid.NewGuid().ToString();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderDetails, providerDetailsLink)
            .AddUrlForRoute(RouteNames.AddProvider, addProvideLink);

        // Act
        var result = sut.Index() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        var model = result!.Model as ProviderDetailsViewModel;
        model!.AddProviderRouteUrl.Should().Be(providerDetailsLink);
        model!.SelectProviderUrl.Should().Be(addProvideLink);
        model.Ukprn.Should().Be(sessionModel.Ukprn);
        model.LegalName!.Should().Be(sessionModel.LegalName);
        model.TradingName!.Should().Be(sessionModel.TradingName);
        model.CompanyNumber!.Should().Be(sessionModel.CompanyNumber);
        model.CharityNumber!.Should().Be(sessionModel.CharityNumber);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
    }

    [Test, MoqAutoData]
    public void Get_Index_SessionMissingData_ReturnsViewWithModel(
        [Frozen] Mock<ISessionService> sessionServiceMock)
    {
        // Arrange
        var sessionModel = new AddProviderSessionModel()
        {
            Ukprn = 12345678,
            LegalName = "LegalName",
            TradingName = null,
            CompanyNumber = null,
            CharityNumber = null
        };

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        var sut = new ProviderDetailsController(sessionServiceMock.Object);
        var addProvideLink = Guid.NewGuid().ToString();
        var providerDetailsLink = Guid.NewGuid().ToString();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderDetails, providerDetailsLink)
            .AddUrlForRoute(RouteNames.AddProvider, addProvideLink);

        // Act
        var result = sut.Index() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        var model = result!.Model as ProviderDetailsViewModel;
        model!.AddProviderRouteUrl.Should().Be(providerDetailsLink);
        model!.SelectProviderUrl.Should().Be(addProvideLink);
        model.Ukprn.Should().Be(sessionModel.Ukprn);
        model.LegalName!.Should().Be(sessionModel.LegalName);
        model.TradingName!.Should().Be("Not applicable");
        model.CompanyNumber!.Should().Be("Not applicable");
        model.CharityNumber!.Should().Be("Not applicable");
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
    }
}

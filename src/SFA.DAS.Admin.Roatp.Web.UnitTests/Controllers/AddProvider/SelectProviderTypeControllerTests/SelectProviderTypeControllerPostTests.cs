using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.SelectProviderTypeControllerTests;
public class SelectProviderTypeControllerPostTests
{
    [Test, MoqAutoData]
    public void Post_Index_SubmitModelIsValidAndNotSupportingProvider_SetsSessionAndRedirectsToCorrectAction(
        [Frozen] Mock<IValidator<SelectProviderTypeSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectProviderTypeController sut)
    {
        // Arrange
        var providerTypeId = 1;
        var sessionModel = new AddProviderSessionModel()
        {
            Ukprn = 12345678,
            LegalName = "LegalName",
            TradingName = "TradingName",
            CompanyNumber = "12345678",
            CharityNumber = "12345678",
            ProviderTypeId = providerTypeId,
            OffersApprenticeships = true
        };

        SelectProviderTypeSubmitModel submitModel = new() { SelectedProviderTypeId = providerTypeId };

        validator.Setup(x => x.Validate(It.Is<SelectProviderTypeSubmitModel>(m => m.SelectedProviderTypeId == submitModel.SelectedProviderTypeId))).Returns(new ValidationResult());

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        // Act
        var result = sut.Index(submitModel);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.SelectOfferApprenticeships);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.ProviderTypeId == sessionModel.ProviderTypeId && m.OffersApprenticeships == sessionModel.OffersApprenticeships)), Times.Once);
    }

    [Test, MoqAutoData]
    public void Post_Index_SubmitModelIsValidAndSupportingProvider_SetsSessionAndRedirectsToCorrectAction(
        [Frozen] Mock<IValidator<SelectProviderTypeSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectProviderTypeController sut)
    {
        // Arrange
        var providerTypeId = 3;
        var sessionModel = new AddProviderSessionModel()
        {
            Ukprn = 12345678,
            LegalName = "LegalName",
            TradingName = "TradingName",
            CompanyNumber = "12345678",
            CharityNumber = "12345678",
            ProviderTypeId = providerTypeId,
            OffersApprenticeships = null
        };

        SelectProviderTypeSubmitModel submitModel = new() { SelectedProviderTypeId = providerTypeId };

        validator.Setup(x => x.Validate(It.Is<SelectProviderTypeSubmitModel>(m => m.SelectedProviderTypeId == submitModel.SelectedProviderTypeId))).Returns(new ValidationResult());

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        // Act
        var result = sut.Index(submitModel);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.SelectOrganisationType);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.ProviderTypeId == sessionModel.ProviderTypeId && m.OffersApprenticeships == sessionModel.OffersApprenticeships)), Times.Once);
    }

    [Test, MoqAutoData]
    public void Post_Index_SubmitModelIsValidAndProviderTypeIdIsDifferentInSession_ClearsSessionPropertiesSetsSessionAndRedirectsToCorrectAction(
        [Frozen] Mock<IValidator<SelectProviderTypeSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectProviderTypeController sut)
    {
        // Arrange
        var providerTypeId = 1;
        var sessionModel = new AddProviderSessionModel()
        {
            Ukprn = 12345678,
            LegalName = "LegalName",
            TradingName = "TradingName",
            CompanyNumber = "12345678",
            CharityNumber = "12345678",
            ProviderTypeId = 2,
            OffersApprenticeships = true,
            OffersApprenticeshipUnits = true
        };

        SelectProviderTypeSubmitModel submitModel = new() { SelectedProviderTypeId = providerTypeId };

        validator.Setup(x => x.Validate(It.Is<SelectProviderTypeSubmitModel>(m => m.SelectedProviderTypeId == submitModel.SelectedProviderTypeId))).Returns(new ValidationResult());

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        // Act
        var result = sut.Index(submitModel);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.SelectOfferApprenticeships);
        sessionModel.OffersApprenticeships.Should().BeNull();
        sessionModel.OffersApprenticeshipUnits.Should().BeNull();
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.ProviderTypeId == sessionModel.ProviderTypeId && m.OffersApprenticeships == sessionModel.OffersApprenticeships)), Times.Once);
    }

    [Test, MoqAutoData]
    public void Post_Index_SubmitModelIsInvalid_ReturnsViewWithErrors(
        [Frozen] Mock<IValidator<SelectProviderTypeSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectProviderTypeController sut)
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

        SelectProviderTypeSubmitModel submitModel = new();

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));

        validator.Setup(x => x.Validate(It.Is<SelectProviderTypeSubmitModel>(m => m == submitModel))).Returns(validationResult);

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        // Act
        var result = sut.Index(submitModel);

        // Assert
        result.Should().NotBeNull();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().NotBeNull();
        viewResult!.Model.Should().BeOfType<SelectProviderTypeViewModel>();
        sut.ModelState.ErrorCount.Should().Be(1);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Never());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.ProviderTypeId == sessionModel.ProviderTypeId)), Times.Never);
    }

    [Test, MoqAutoData]
    public void Post_Index_SubmitModelIsValidAndRedirectedFromSummaryPageIsTrue_SetsSessionAndRedirectsToProviderDetailsSummary(
        [Frozen] Mock<IValidator<SelectProviderTypeSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectProviderTypeController sut)
    {
        // Arrange
        var providerTypeId = 3;
        var sessionModel = new AddProviderSessionModel()
        {
            Ukprn = 12345678,
            LegalName = "LegalName",
            TradingName = "TradingName",
            CompanyNumber = "12345678",
            CharityNumber = "12345678",
            ProviderTypeId = providerTypeId,
            OffersApprenticeships = null,
            RedirectedFromSummaryPage = true
        };

        SelectProviderTypeSubmitModel submitModel = new() { SelectedProviderTypeId = providerTypeId };

        validator.Setup(x => x.Validate(It.Is<SelectProviderTypeSubmitModel>(m => m.SelectedProviderTypeId == submitModel.SelectedProviderTypeId))).Returns(new ValidationResult());

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        // Act
        var result = sut.Index(submitModel);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.ProviderDetailsSummary);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.ProviderTypeId == sessionModel.ProviderTypeId && m.OffersApprenticeships == sessionModel.OffersApprenticeships)), Times.Never);
    }
}
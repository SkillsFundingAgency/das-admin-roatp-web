using AutoFixture.NUnit3;
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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.SelectOfferApprenticeshipUnitsControllerTests;
public class SelectOfferApprenticeshipUnitsControllerPostTests
{
    [Test, MoqAutoData]
    public void Post_Index_SubmitModelIsValidAndOfferApprenticeshipUnitsTrue_SetsSessionAndRedirectsToCorrectAction(
        [Frozen] Mock<IValidator<OfferApprenticeshipUnitsSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectOfferApprenticeshipUnitsController sut)
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
            OfferApprenticeships = true,
            OfferApprenticeshipUnits = true
        };

        OfferApprenticeshipUnitsSubmitModel submitModel = new() { ApprenticeshipUnitsSelectionId = true };

        validator.Setup(x => x.Validate(It.Is<OfferApprenticeshipUnitsSubmitModel>(m => m.ApprenticeshipUnitsSelectionId == submitModel.ApprenticeshipUnitsSelectionId))).Returns(new ValidationResult());

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        // Act
        var result = sut.Index(submitModel);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.SelectOfferApprenticeshipUnits);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.OfferApprenticeshipUnits == sessionModel.OfferApprenticeshipUnits)), Times.Once);
    }

    [Test, MoqAutoData]
    public void Post_Index_SubmitModelIsValidAndOfferApprenticeshipUnitsFalse_SetsSessionAndRedirectsToCorrectAction(
        [Frozen] Mock<IValidator<OfferApprenticeshipUnitsSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectOfferApprenticeshipUnitsController sut)
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
            OfferApprenticeships = false,
            OfferApprenticeshipUnits = false
        };

        OfferApprenticeshipUnitsSubmitModel submitModel = new() { ApprenticeshipUnitsSelectionId = false };

        validator.Setup(x => x.Validate(It.Is<OfferApprenticeshipUnitsSubmitModel>(m => m.ApprenticeshipUnitsSelectionId == submitModel.ApprenticeshipUnitsSelectionId))).Returns(new ValidationResult());

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        // Act
        var result = sut.Index(submitModel);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.SelectOfferApprenticeshipUnits);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.OfferApprenticeshipUnits == sessionModel.OfferApprenticeshipUnits)), Times.Once);
    }

    [Test, MoqAutoData]
    public void Post_Index_SubmitModelIsInvalid_ReturnsViewWithErrors(
        [Frozen] Mock<IValidator<OfferApprenticeshipUnitsSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectOfferApprenticeshipUnitsController sut)
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

        OfferApprenticeshipUnitsSubmitModel submitModel = new();

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));

        validator.Setup(x => x.Validate(It.Is<OfferApprenticeshipUnitsSubmitModel>(m => m == submitModel))).Returns(validationResult);

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        // Act
        var result = sut.Index(submitModel);

        // Assert
        result.Should().NotBeNull();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().NotBeNull();
        viewResult!.Model.Should().BeOfType<OfferApprenticeshipUnitsViewModel>();
        sut.ModelState.ErrorCount.Should().Be(1);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.OfferApprenticeships == sessionModel.OfferApprenticeships)), Times.Never);
    }

    [Test, MoqAutoData]
    public void Post_Index_SessionIsNull_RedirectsToHome(
    [Frozen] Mock<ISessionService> sessionServiceMock,
    [Greedy] SelectOfferApprenticeshipUnitsController sut)
    {
        // Arrange
        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(() => null!);
        OfferApprenticeshipUnitsSubmitModel submitModel = new();

        // Act
        var result = sut.Index(submitModel);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result! as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult!.RouteName.Should().Be(RouteNames.Home);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
    }
}

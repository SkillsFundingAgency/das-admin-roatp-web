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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.SelectOfferApprenticeshipsControllerTests;
public class SelectOfferApprenticeshipsControllerPostTests
{
    [Test, MoqAutoData]
    public void Post_Index_SubmitModelIsValid_SetsSessionAndRedirectsToOfferAppreticeshipUnits(
        [Frozen] Mock<IValidator<OfferApprenticeshipsSubmitModel>> validator,
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
            OffersApprenticeships = true,
            OffersApprenticeshipUnits = true
        };

        OfferApprenticeshipsSubmitModel submitModel = new() { IsApprenticeshipsOffered = true };

        validator.Setup(x => x.Validate(It.Is<OfferApprenticeshipsSubmitModel>(m => m.IsApprenticeshipsOffered == submitModel.IsApprenticeshipsOffered))).Returns(new ValidationResult());

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
            m.OffersApprenticeships == sessionModel.OffersApprenticeships && m.OffersApprenticeshipUnits == sessionModel.OffersApprenticeshipUnits)), Times.Once);
    }

    [Test, MoqAutoData]
    public void Post_Index_SubmitModelIsInvalid_ReturnsViewWithErrors(
        [Frozen] Mock<IValidator<OfferApprenticeshipsSubmitModel>> validator,
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
            ProviderTypeId = null
        };

        OfferApprenticeshipsSubmitModel submitModel = new();

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));

        validator.Setup(x => x.Validate(It.Is<OfferApprenticeshipsSubmitModel>(m => m == submitModel))).Returns(validationResult);

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        // Act
        var result = sut.Index(submitModel);

        // Assert
        result.Should().NotBeNull();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().NotBeNull();
        viewResult!.Model.Should().BeOfType<OfferApprenticeshipsViewModel>();
        sut.ModelState.ErrorCount.Should().Be(1);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.OffersApprenticeships == sessionModel.OffersApprenticeships)), Times.Never);
    }

    [Test, MoqAutoData]
    public void Post_Index_SessionIsNull_RedirectsToHome(
   [Frozen] Mock<ISessionService> sessionServiceMock,
   [Greedy] SelectOfferApprenticeshipsController sut)
    {
        // Arrange
        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(() => null!);
        OfferApprenticeshipsSubmitModel submitModel = new();


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

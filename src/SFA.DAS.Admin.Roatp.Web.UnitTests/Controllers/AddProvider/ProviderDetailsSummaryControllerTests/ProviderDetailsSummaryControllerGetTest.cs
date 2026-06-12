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
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.ProviderDetailsSummaryControllerTests;
public class ProviderDetailsSummaryControllerGetTest
{
    [Test, MoqAutoData]
    public void Get_Index_ViewModelIsValid_ReturnsView(
        [Frozen] Mock<IValidator<ProviderDetailsSummaryViewModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ProviderDetailsSummaryController sut,
        AddProviderSessionModel sessionModel
        )
    {
        // Arrange
        string manageProviderLink = Guid.NewGuid().ToString();
        string providerTypeChangeLink = Guid.NewGuid().ToString();
        string offersApprenticeshipsChangeLink = Guid.NewGuid().ToString();
        string offersApprenticeshipUnitsChangeLink = Guid.NewGuid().ToString();
        string organisationTypeChangeLink = Guid.NewGuid().ToString();

        var submitModel = new ProviderDetailsSummaryViewModel() { Ukprn = 12345, IsSupportingProvider = false, OffersApprenticeshipsText = "Yes", OffersApprenticeshipUnitsText = "No" };

        validator.Setup(x => x.Validate(It.Is<ProviderDetailsSummaryViewModel>(m => m.IsSupportingProvider == submitModel.IsSupportingProvider))).Returns(new ValidationResult());

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.Home, manageProviderLink)
            .AddUrlForRoute(RouteNames.SelectProviderType, providerTypeChangeLink)
            .AddUrlForRoute(RouteNames.SelectOfferApprenticeships, offersApprenticeshipsChangeLink)
            .AddUrlForRoute(RouteNames.SelectOfferApprenticeshipUnits, offersApprenticeshipUnitsChangeLink)
            .AddUrlForRoute(RouteNames.SelectOrganisationType, organisationTypeChangeLink);

        // Act
        var result = sut.Index() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        result!.Model.Should().BeOfType<ProviderDetailsSummaryViewModel>();
        var model = result!.Model as ProviderDetailsSummaryViewModel;
        sut.ModelState.ErrorCount.Should().Be(0);
        model!.Ukprn.Should().Be(sessionModel.Ukprn);
        model!.LegalName.Should().BeEquivalentTo(sessionModel.LegalName);
        model.ManageProviderLink.Should().Be(manageProviderLink);
        model.ProviderTypeChangeLink.Should().Be(providerTypeChangeLink);
        model.OffersApprenticeshipsChangeLink.Should().Be(offersApprenticeshipsChangeLink);
        model.OffersApprenticeshipUnitsChangeLink.Should().Be(offersApprenticeshipUnitsChangeLink);
        model.OrganisationTypeChangeLink.Should().Be(organisationTypeChangeLink);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
    }

    [Test, MoqAutoData]
    public void Get_Index_ViewModelIsInvalid_ReturnsViewWithErrors(
        [Frozen] Mock<IValidator<ProviderDetailsSummaryViewModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ProviderDetailsSummaryController sut,
        AddProviderSessionModel sessionModel,
        string manageProviderLink)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));

        validator.Setup(x => x.Validate(It.IsAny<ProviderDetailsSummaryViewModel>())).Returns(validationResult);

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.Home, manageProviderLink);

        // Act
        var result = sut.Index();

        // Assert
        result.Should().NotBeNull();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().NotBeNull();
        viewResult!.Model.Should().BeOfType<ProviderDetailsSummaryViewModel>();
        sut.ModelState.ErrorCount.Should().Be(1);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
    }

    [Test, MoqAutoData]
    public void Get_AddProviderConfirmation_ReturnsView(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ProviderDetailsSummaryController sut,
        AddProviderSessionModel sessionModel)
    {
        // Arrange
        sessionModel.OrganisationSubmitted = true;

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        var dashboardLink = Guid.NewGuid().ToString();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.Home, dashboardLink);

        // Act
        var result = sut.AddProviderConfirmation() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        var model = result!.Model as AddProviderConfirmationViewModel;
        model!.DashboardLink.Should().Be(dashboardLink);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddProvider), Times.Once());
    }

    [Test, MoqAutoData]
    public void Get_AddProviderConfirmation_OrganisationSubmittedIsFalse_RedirectsToHome(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ProviderDetailsSummaryController sut,
        AddProviderSessionModel sessionModel)
    {
        // Arrange
        sessionModel.OrganisationSubmitted = false;

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        var dashboardLink = Guid.NewGuid().ToString();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.Home, dashboardLink);

        // Act
        var result = sut.AddProviderConfirmation();

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.Home);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddProvider), Times.Never());
    }
}

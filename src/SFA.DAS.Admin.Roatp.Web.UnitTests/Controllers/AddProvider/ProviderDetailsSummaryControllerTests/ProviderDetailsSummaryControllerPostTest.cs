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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.ProviderDetailsSummaryControllerTests;
public class ProviderDetailsSummaryControllerPostTest
{
    [Test, MoqAutoData]
    public async Task Post_Index_ViewModelIsValid_RedirectsToCorrectAction(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<ProviderDetailsSummaryViewModel>> validator,
        [Frozen] Mock<IPostOrganisationService> postOrganisationService,
        [Greedy] ProviderDetailsSummaryController sut,
        AddProviderSessionModel sessionModel,
        CancellationToken cancellationToken)
    {
        var viewModel = new ProviderDetailsSummaryViewModel() { Ukprn = 12345, IsSupportingProvider = false, OffersApprenticeshipsText = "Yes", OffersApprenticeshipUnitsText = "No" };

        validator.Setup(x => x.Validate(It.Is<ProviderDetailsSummaryViewModel>(m => m.IsSupportingProvider == viewModel.IsSupportingProvider))).Returns(new ValidationResult());

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        postOrganisationService.Setup(p => p.PostOrganisation(It.IsAny<AddProviderSessionModel>(), cancellationToken)).Returns(Task.CompletedTask);

        // Act
        var result = await sut.Index(viewModel, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.AddProviderConfirmation);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.OrganisationSubmitted == sessionModel.OrganisationSubmitted)), Times.Once);
        postOrganisationService.Verify(p => p.PostOrganisation(sessionModel, cancellationToken), Times.Once());
        sessionModel.OrganisationSubmitted.Should().Be(true);
    }

    [Test, MoqAutoData]
    public async Task Post_Index_ViewModelIsInalid_RedirectsToProviderDetailsSummaryGet(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<ProviderDetailsSummaryViewModel>> validator,
        [Frozen] Mock<IPostOrganisationService> postOrganisationService,
        [Greedy] ProviderDetailsSummaryController sut,
        AddProviderSessionModel sessionModel,
        CancellationToken cancellationToken)
    {
        var viewModel = new ProviderDetailsSummaryViewModel() { Ukprn = 12345 };

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));

        validator.Setup(x => x.Validate(It.Is<ProviderDetailsSummaryViewModel>((m => m.IsSupportingProvider == viewModel.IsSupportingProvider)))).Returns(validationResult);

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        postOrganisationService.Setup(p => p.PostOrganisation(It.IsAny<AddProviderSessionModel>(), cancellationToken)).Returns(Task.CompletedTask);

        // Act
        var result = await sut.Index(viewModel, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.ProviderDetailsSummary);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Never());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.OrganisationSubmitted == sessionModel.OrganisationSubmitted)), Times.Never);
        postOrganisationService.Verify(p => p.PostOrganisation(sessionModel, cancellationToken), Times.Never());
        sessionModel.OrganisationSubmitted.Should().Be(false);
    }
}

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
        AddProviderSessionModel sessionModel,
        string manageProviderLink)
    {
        var submitModel = new ProviderDetailsSummaryViewModel() { IsSupportingProvider = false, OffersApprenticeshipsText = "Yes", OffersApprenticeshipUnitsText = "No" };

        validator.Setup(x => x.Validate(It.Is<ProviderDetailsSummaryViewModel>(m => m.IsSupportingProvider == submitModel.IsSupportingProvider))).Returns(new ValidationResult());

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.Home, manageProviderLink);

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
        var viewModel = new ProviderDetailsSummaryViewModel();

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));

        validator.Setup(x => x.Validate(It.Is<ProviderDetailsSummaryViewModel>((m => m.IsSupportingProvider == viewModel.IsSupportingProvider)))).Returns(validationResult);

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
}

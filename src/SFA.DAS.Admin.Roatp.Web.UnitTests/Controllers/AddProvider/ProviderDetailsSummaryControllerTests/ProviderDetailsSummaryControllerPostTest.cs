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
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.ProviderDetailsSummaryControllerTests;
public class ProviderDetailsSummaryControllerPostTest
{
    [Test, MoqAutoData]
    public void Post_Index_ViewModelIsValid_RedirectsToCorrectAction(
        [Frozen] Mock<IValidator<ProviderDetailsSummaryViewModel>> validator,
        [Greedy] ProviderDetailsSummaryController sut,
        AddProviderSessionModel sessionModel)
    {
        var viewModel = new ProviderDetailsSummaryViewModel() { IsSupportingProvider = false, OffersApprenticeshipsText = "Yes", OffersApprenticeshipUnitsText = "No" };

        validator.Setup(x => x.Validate(It.Is<ProviderDetailsSummaryViewModel>(m => m.IsSupportingProvider == viewModel.IsSupportingProvider))).Returns(new ValidationResult());

        // Act
        var result = sut.Index(viewModel);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.ProviderDetailsSummary);
    }

    [Test, MoqAutoData]
    public void Post_Index_ViewModelIsInalid_RedirectsToProviderDetailsSummaryGet(
        [Frozen] Mock<IValidator<ProviderDetailsSummaryViewModel>> validator,
        [Greedy] ProviderDetailsSummaryController sut,
        AddProviderSessionModel sessionModel)
    {
        var viewModel = new ProviderDetailsSummaryViewModel();

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));

        validator.Setup(x => x.Validate(It.Is<ProviderDetailsSummaryViewModel>((m => m.IsSupportingProvider == viewModel.IsSupportingProvider)))).Returns(validationResult);

        // Act
        var result = sut.Index(viewModel);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.ProviderDetailsSummary);
    }
}

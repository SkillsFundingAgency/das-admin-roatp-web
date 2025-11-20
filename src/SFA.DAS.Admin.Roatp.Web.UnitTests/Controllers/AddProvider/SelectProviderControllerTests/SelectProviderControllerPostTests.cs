using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Testing.AutoFixture;
using System.Net;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.SelectProviderControllerTests;
public class SelectProviderControllerPostTests
{
    [Test, MoqAutoData]
    public async Task Post_Index_SubmitModelIsValid_RedirectsToCorrectAction(
        [Frozen] Mock<IValidator<AddProviderSubmitModel>> validator,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        CancellationToken cancellationToken)
    {
        // Arrange
        AddProviderSubmitModel viewModel = new() { Ukprn = "12345678" };
        validator.Setup(x => x.Validate(It.Is<AddProviderSubmitModel>(m => m.Ukprn == viewModel.Ukprn))).Returns(new ValidationResult());

        outerApiClient.Setup(x => x.GetOrganisation(int.Parse(viewModel.Ukprn!), cancellationToken)).ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        outerApiClient.Setup(x => x.GetUkrlp(int.Parse(viewModel.Ukprn!), cancellationToken)).ReturnsAsync(new ApiResponse<GetUkrlpResponse>(new HttpResponseMessage(HttpStatusCode.OK), new GetUkrlpResponse(), new RefitSettings(), null));

        SelectProviderController sut = new(validator.Object, outerApiClient.Object);

        // Act
        var result = await sut.Index(viewModel, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.AddProvider);
        outerApiClient.Verify(x => x.GetOrganisation(int.Parse(viewModel.Ukprn!), cancellationToken), Times.Once);
        outerApiClient.Verify(x => x.GetUkrlp(int.Parse(viewModel.Ukprn!), cancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Post_Index_SubmitModelIsInvalid_ReturnsViewWithErrors(
    [Frozen] Mock<IValidator<AddProviderSubmitModel>> validator,
    [Frozen] Mock<IOuterApiClient> outerApiClient,
    CancellationToken cancellationToken)
    {
        // Arrange
        AddProviderSubmitModel viewModel = new() { Ukprn = "12345678" };
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));
        validator.Setup(x => x.Validate(It.Is<AddProviderSubmitModel>(m => m.Ukprn == viewModel.Ukprn))).Returns(validationResult);

        SelectProviderController sut = new(validator.Object, outerApiClient.Object);

        // Act
        var result = await sut.Index(viewModel, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().NotBeNull();
        viewResult!.Model.Should().BeOfType<AddProviderViewModel>();
        sut.ModelState.ErrorCount.Should().Be(1);
        outerApiClient.Verify(x => x.GetOrganisation(int.Parse(viewModel.Ukprn!), cancellationToken), Times.Never);
        outerApiClient.Verify(x => x.GetUkrlp(int.Parse(viewModel.Ukprn!), cancellationToken), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Post_Index_ExistingOrganisation_ReturnsViewWithErrors(
    [Frozen] Mock<IValidator<AddProviderSubmitModel>> validator,
    [Frozen] Mock<IOuterApiClient> outerApiClient,
    CancellationToken cancellationToken)
    {
        // Arrange
        AddProviderSubmitModel viewModel = new() { Ukprn = "12345678" };
        validator.Setup(x => x.Validate(It.Is<AddProviderSubmitModel>(m => m.Ukprn == viewModel.Ukprn))).Returns(new ValidationResult());

        outerApiClient.Setup(x => x.GetOrganisation(int.Parse(viewModel.Ukprn!), cancellationToken)).ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), new GetOrganisationResponse(), new RefitSettings(), null));

        SelectProviderController sut = new(validator.Object, outerApiClient.Object);

        // Act
        var result = await sut.Index(viewModel, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().NotBeNull();
        viewResult!.Model.Should().BeOfType<AddProviderViewModel>();
        sut.ModelState.ErrorCount.Should().Be(1);
        outerApiClient.Verify(x => x.GetOrganisation(int.Parse(viewModel.Ukprn!), cancellationToken), Times.Once);
        outerApiClient.Verify(x => x.GetUkrlp(int.Parse(viewModel.Ukprn!), cancellationToken), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Post_Index_UkrlpNotFound_RedirectsToCorrectAction(
        [Frozen] Mock<IValidator<AddProviderSubmitModel>> validator,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        CancellationToken cancellationToken)
    {
        // Arrange
        AddProviderSubmitModel viewModel = new() { Ukprn = "12345678" };
        validator.Setup(x => x.Validate(It.Is<AddProviderSubmitModel>(m => m.Ukprn == viewModel.Ukprn))).Returns(new ValidationResult());

        outerApiClient.Setup(x => x.GetOrganisation(int.Parse(viewModel.Ukprn!), cancellationToken)).ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        outerApiClient.Setup(x => x.GetUkrlp(int.Parse(viewModel.Ukprn!), cancellationToken)).ReturnsAsync(new ApiResponse<GetUkrlpResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetUkrlpResponse(), new RefitSettings(), null));

        SelectProviderController sut = new(validator.Object, outerApiClient.Object);

        // Act
        var result = await sut.Index(viewModel, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.ProviderNotFoundInUkrlp);
        outerApiClient.Verify(x => x.GetOrganisation(int.Parse(viewModel.Ukprn!), cancellationToken), Times.Once);
        outerApiClient.Verify(x => x.GetUkrlp(int.Parse(viewModel.Ukprn!), cancellationToken), Times.Once);
    }
}
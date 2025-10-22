using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.SelectTrainingProviderControllerTests;
public class SelectTrainingProviderControllerPostTests
{
    [Test, MoqAutoData]
    public async Task And_SubmitViewModel_Is_Valid_Reroutes_To_Expected_Action(
        SelectTrainingProviderSubmitViewModel viewModel,
        [Frozen] Mock<IValidator<SelectTrainingProviderSubmitViewModel>> validator,
        [Greedy] SelectTrainingProviderController controller,
        CancellationToken cancellationToken)
    {
        validator.Setup(x => x.Validate(viewModel)).Returns(new ValidationResult());

        var actual = await controller.Index(viewModel, cancellationToken);

        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
    }

    [Test, MoqAutoData]
    public async Task And_SubmitViewModel_Is_Invalid_Reloads_View(
        SelectTrainingProviderSubmitViewModel viewModel,
        [Frozen] Mock<IValidator<SelectTrainingProviderSubmitViewModel>> validator,
        [Greedy] SelectTrainingProviderController controller,
        CancellationToken cancellationToken)
    {
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));

        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);

        var actual = await controller.Index(viewModel, cancellationToken);

        actual.Should().NotBeNull();
        var result = actual! as ViewResult;
        result.Should().NotBeNull();
        var model = result!.Model as SelectTrainingProviderViewModel;

        model.Should().NotBeNull();
    }

    [Test, MoqAutoData]
    public async Task And_SubmitViewModel_Is_Valid_No_Matching_Organisation_Redirect_To_Home(
        SelectTrainingProviderSubmitViewModel viewModel,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SelectTrainingProviderSubmitViewModel>> validator,
        [Greedy] SelectTrainingProviderController controller,
        CancellationToken cancellationToken)
    {
        validator.Setup(x => x.Validate(viewModel)).Returns(new ValidationResult());
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((GetOrganisationResponse)null!);

        var actual = await controller.Index(viewModel, cancellationToken);

        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }
}

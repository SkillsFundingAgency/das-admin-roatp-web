using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.RemovalReasonUpdateControllerTests;
public class RemovalReasonUpdateControllerPostTests
{
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RemovalReasonUpdateController sut,
        RemovalReasonUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((GetOrganisationResponse)null!);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public async Task And_SubmitViewModel_Is_Invalid_Reloads_View(
        RemovalReasonUpdateViewModel viewModel,
        [Frozen] Mock<IValidator<RemovalReasonUpdateViewModel>> validator,
        [Greedy] RemovalReasonUpdateController controller,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));

        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);

        var actual = await controller.Index(ukprn, viewModel, cancellationToken);

        actual.Should().NotBeNull();
        var result = actual! as ViewResult;
        result.Should().NotBeNull();
        var model = result!.Model as RemovalReasonUpdateViewModel;
        model.Should().NotBeNull();
    }

    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_NoRemovedReasonChange_RedirectToProviderSummary(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<RemovalReasonUpdateViewModel>> validator,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] RemovalReasonUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        RemovalReasonUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), cancellationToken))!
            .ReturnsAsync(getOrganisationResponse);
        var validationResult = new ValidationResult();
        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);
        organisationPatchService.Setup(x => x.OrganisationPatched(ukprn, It.IsAny<GetOrganisationResponse>(), It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(false);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
    }

    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_RemovedReasonChange_RedirectToProviderStatusUpdateConfirmed(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<RemovalReasonUpdateViewModel>> validator,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] RemovalReasonUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        RemovalReasonUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), cancellationToken))!
            .ReturnsAsync(getOrganisationResponse);
        var validationResult = new ValidationResult();
        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);
        organisationPatchService.Setup(x => x.OrganisationPatched(ukprn, It.IsAny<GetOrganisationResponse>(), It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(true);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderStatusUpdateConfirmed);
    }
}

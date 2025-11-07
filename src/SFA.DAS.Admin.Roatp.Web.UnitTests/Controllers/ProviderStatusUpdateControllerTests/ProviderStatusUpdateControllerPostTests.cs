using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ProviderStatusUpdateControllerTests;
public class ProviderStatusUpdateControllerPostTests
{
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderStatusUpdateController sut,
        OrganisationStatusUpdateViewModel viewModel,
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

    [Test]
    [MoqInlineAutoData(OrganisationStatus.Active)]
    [MoqInlineAutoData(OrganisationStatus.ActiveNoStarts)]
    [MoqInlineAutoData(OrganisationStatus.OnBoarding)]
    public async Task Get_MatchingDetails_NoStatusChange_RedirectToProviderSummary(
        OrganisationStatus status,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] ProviderStatusUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        OrganisationStatusUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.Status = status;
        viewModel.OrganisationStatus = getOrganisationResponse.Status;
        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        organisationPatchService.Setup(x => x.OrganisationPatched(ukprn, It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(false);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
    }

    [Test]
    [MoqInlineAutoData(OrganisationStatus.Active)]
    [MoqInlineAutoData(OrganisationStatus.ActiveNoStarts)]
    [MoqInlineAutoData(OrganisationStatus.OnBoarding)]
    public async Task Get_MatchingDetails_StatusChange_RedirectToProviderStatusUpdateConfirmed(
        OrganisationStatus status,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] ProviderStatusUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        OrganisationStatusUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.Status = status;
        viewModel.OrganisationStatus = getOrganisationResponse.Status;
        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        organisationPatchService.Setup(x => x.OrganisationPatched(ukprn, It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(true);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderStatusUpdateConfirmed);
    }

    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_RedirectBasedOnStatusDifferenceWithRemoved(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderStatusUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        OrganisationStatusUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        OrganisationStatus responseStatus = OrganisationStatus.Active;

        OrganisationStatus submitModelStatus = OrganisationStatus.Removed;

        getOrganisationResponse.Ukprn = ukprn;
        getOrganisationResponse.Status = responseStatus;
        viewModel.OrganisationStatus = submitModelStatus;

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();

        result!.RouteName.Should().Be(RouteNames.ProviderRemovalReasonUpdate);
    }
}

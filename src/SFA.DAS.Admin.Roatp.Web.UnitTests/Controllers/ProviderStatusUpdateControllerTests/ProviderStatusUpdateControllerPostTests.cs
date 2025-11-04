using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
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

    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_NoStatusChange_RedirectToProviderSummary(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderStatusUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        OrganisationStatusUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        viewModel.OrganisationStatus = getOrganisationResponse.Status;
        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
    }

    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_RedirectBasedOnStatusDifference(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderStatusUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        OrganisationStatusUpdateViewModel viewModel,
        OrganisationStatus responseStatus,
        OrganisationStatus submitModelStatus,
        int ukprn,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.Ukprn = ukprn;
        getOrganisationResponse.Status = responseStatus;
        viewModel.OrganisationStatus = submitModelStatus;

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        var patchDoc = new JsonPatchDocument<PatchOrganisationModel>();
        patchDoc.Replace(o => o.Status, submitModelStatus);
        var expectedOperation = patchDoc.Operations.First();

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();

        if (responseStatus == submitModelStatus)
        {
            result!.RouteName.Should().Be(RouteNames.ProviderSummary);
            outerApiClientMock.Verify(x => x.PatchOrganisation(ukprn.ToString(), It.IsAny<string>(), It.IsAny<JsonPatchDocument<PatchOrganisationModel>>(), cancellationToken), Times.Never);
        }

        if (responseStatus != submitModelStatus)
        {
            result!.RouteName.Should().Be(RouteNames.ProviderStatusUpdateConfirmed);
            outerApiClientMock.Verify(x => x.PatchOrganisation(ukprn.ToString(), It.IsAny<string>(),
                It.Is<JsonPatchDocument<PatchOrganisationModel>>(document =>
                    document.Operations.Count >= 1
                    && document.Operations.Any(o =>
                        o.OperationType == expectedOperation.OperationType
                        && o.path == expectedOperation.path
                        && o.value.Equals(expectedOperation.value))),
                    cancellationToken), Times.Once);
        }
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

        var patchDoc = new JsonPatchDocument<PatchOrganisationModel>();
        patchDoc.Replace(o => o.Status, submitModelStatus);
        var removedReasonIdOther = 12;
        patchDoc.Replace(o => o.RemovedReasonId, removedReasonIdOther);

        var expectedOperation = patchDoc.Operations.First();
        var expectedOperationRemovedReasonId = patchDoc.Operations.Skip(1).First();

        getOrganisationResponse.Ukprn = ukprn;
        getOrganisationResponse.Status = responseStatus;
        viewModel.OrganisationStatus = submitModelStatus;

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();

        result!.RouteName.Should().Be(RouteNames.ProviderStatusUpdateConfirmed);
        outerApiClientMock.Verify(x => x.PatchOrganisation(ukprn.ToString(), It.IsAny<string>(),
            It.Is<JsonPatchDocument<PatchOrganisationModel>>(document =>
                document.Operations.Count == 2
                && document.Operations.Any(o =>
                    o.OperationType == expectedOperation.OperationType
                    && o.path == expectedOperation.path
                    && o.value.Equals(expectedOperation.value))
                && document.Operations.Any(o =>
                    o.OperationType == expectedOperationRemovedReasonId.OperationType
                    && o.path == expectedOperationRemovedReasonId.path
                    && o.value.Equals(expectedOperationRemovedReasonId.value))
                ),
            cancellationToken), Times.Once);
    }
}

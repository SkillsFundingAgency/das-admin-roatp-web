using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Testing.AutoFixture;
using System.Net;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.RemovalReasonUpdateControllerTests;
public class RemovalReasonUpdateControllerGetTests
{
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RemovalReasonUpdateController sut,
        int ukprn,
        CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_BuildViewModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RemovalReasonUpdateController sut,
        string selectOrganisationLink,
        GetOrganisationResponse getOrganisationResponse,
        GetRemovalReasonsResponse getRemovalReasonsResponse,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var removedReasonId = getRemovalReasonsResponse.ReasonsForRemoval[0].Id;
        getOrganisationResponse.RemovedReasonId = removedReasonId;
        getOrganisationResponse.Ukprn = ukprn;

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        outerApiClientMock.Setup(x => x.GetRemovalReasons(cancellationToken)).ReturnsAsync(getRemovalReasonsResponse);

        var expectedReasonsForRemoval = getRemovalReasonsResponse.ReasonsForRemoval.OrderBy(r => r.Description).ToList();
        foreach (var reason in expectedReasonsForRemoval)
        {
            reason.IsSelected = reason.Id == removedReasonId;
        }

        var actual = await sut.Index(ukprn, cancellationToken) as ViewResult;
        actual.Should().NotBeNull();
        var model = actual.Model as RemovalReasonUpdateViewModel;
        model.Should().NotBeNull();
        model.RemovalReasonId.Should().Be(getOrganisationResponse.RemovedReasonId);
        model.RemovedReasons.Should().BeEquivalentTo(expectedReasonsForRemoval);
    }
}

using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Testing.AutoFixture;
using System.Net;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.OrganisationTypeUpdateControllerTests;
public class OrganisationTypeUpdateControllerPostTests
{
    private const int Ukprn = 10019900;

    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] OrganisationTypeUpdateController sut,
        OrganisationTypeUpdateViewModel viewModel,
        int ukprn)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, viewModel, new CancellationToken());
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public async Task Get_NoOrganisationTypeChange_RedirectToProviderSummary(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] OrganisationTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        OrganisationTypeUpdateViewModel viewModel,
        OrganisationType organisationType)
    {
        var cancellationToken = new CancellationToken();
        getOrganisationResponse.OrganisationType = organisationType.ToString();
        viewModel.OrganisationTypeId = getOrganisationResponse.OrganisationTypeId;
        getOrganisationResponse.Ukprn = Ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(Ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.GetOrganisation(Ukprn, cancellationToken), Times.Once);
        organisationPatchService.Verify(o => o.OrganisationPatched(Ukprn,
            It.IsAny<GetOrganisationResponse>(),
            It.Is<PatchOrganisationModel>(p => p.OrganisationTypeId == viewModel.OrganisationTypeId),
            cancellationToken), Times.Once);
    }
}
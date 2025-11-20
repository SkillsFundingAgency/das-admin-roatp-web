using AutoFixture.NUnit3;
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
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] OrganisationTypeUpdateController sut,
        OrganisationTypeUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
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
        OrganisationType organisationType,
        int ukprn,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.OrganisationType = organisationType.ToString();
        viewModel.OrganisationTypeId = getOrganisationResponse.OrganisationTypeId;
        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.GetOrganisation(ukprn, cancellationToken), Times.Once);
        organisationPatchService.Verify(o => o.OrganisationPatched(ukprn,
            It.IsAny<GetOrganisationResponse>(),
            It.Is<PatchOrganisationModel>(p => p.OrganisationTypeId == viewModel.OrganisationTypeId),
            cancellationToken), Times.Once);
    }
}

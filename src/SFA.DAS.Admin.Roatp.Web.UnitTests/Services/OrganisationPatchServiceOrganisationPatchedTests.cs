using System.Net;
using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;
public class OrganisationPatchServiceOrganisationPatchedTests
{
    [Test, MoqAutoData]
    public async Task OrganisationPatched_NoChange(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] OrganisationPatchService sut,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        CancellationToken cancellationToken
    )
    {
        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);
        PatchOrganisationModel patchModel = getOrganisationResponse;
        var changed = await sut.OrganisationPatched(ukprn, patchModel, cancellationToken);


        changed.Should().Be(false);
        outerApiClientMock.Verify(c => c.GetOrganisation(ukprn.ToString(), cancellationToken), Times.Once);
        outerApiClientMock.Verify(
            c => c.PatchOrganisation(ukprn.ToString(), It.IsAny<string>(),
                It.IsAny<JsonPatchDocument<PatchOrganisationModel>>(), cancellationToken), Times.Never);
    }

    [TestCaseSource(nameof(_patchRequiredTestCases))]
    public async Task OrganisationPatched_ChangeExpected(
        PatchOrganisationModel organisationDetails,
        PatchOrganisationModel patchModelApplied
    )
    {
        Mock<IHttpContextAccessor> context = new Mock<IHttpContextAccessor>();
        Mock<IOuterApiClient> outerApiClientMock = new Mock<IOuterApiClient>();
        GetOrganisationResponse getOrganisationResponse = new GetOrganisationResponse();
        int ukprn = 11111111;
        string userDisplayName = "user name";
        CancellationToken cancellationToken = new CancellationToken();

        getOrganisationResponse.Ukprn = ukprn;
        getOrganisationResponse.Status = organisationDetails.Status;
        getOrganisationResponse.ProviderType = organisationDetails.ProviderType;
        getOrganisationResponse.RemovedReasonId = organisationDetails.RemovedReasonId;
        getOrganisationResponse.OrganisationTypeId = organisationDetails.OrganisationTypeId;

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        ApiResponse<HttpStatusCode> apiResponse = new ApiResponse<HttpStatusCode>(new HttpResponseMessage(HttpStatusCode.NoContent), HttpStatusCode.NoContent, new RefitSettings(), null);

        outerApiClientMock.Setup(x => x.PatchOrganisation(ukprn.ToString(), It.IsAny<string>(), It.IsAny<JsonPatchDocument<PatchOrganisationModel>>(), cancellationToken))!
            .ReturnsAsync(apiResponse);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new("UserDisplayName",userDisplayName)
            },
            "mock"));

        context.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        OrganisationPatchService sut = new OrganisationPatchService(outerApiClientMock.Object, context.Object);
        var changed = await sut.OrganisationPatched(ukprn, patchModelApplied, cancellationToken);

        changed.Should().Be(true);
        outerApiClientMock.Verify(c => c.GetOrganisation(ukprn.ToString(), cancellationToken), Times.Once);
        outerApiClientMock.Verify(
            c => c.PatchOrganisation(ukprn.ToString(), It.IsAny<string>(),
                It.IsAny<JsonPatchDocument<PatchOrganisationModel>>(), cancellationToken), Times.Once);
    }

    private static object[] _patchRequiredTestCases =
    {
        new object[]
        {
            new PatchOrganisationModel{ Status = OrganisationStatus.Active, OrganisationTypeId = 1, ProviderType = ProviderType.Main, RemovedReasonId = null},
            new PatchOrganisationModel{ Status = OrganisationStatus.Removed,OrganisationTypeId = 1, ProviderType = ProviderType.Main, RemovedReasonId = null},
        },
        new object[]
        {
            new PatchOrganisationModel{ Status = OrganisationStatus.Active, OrganisationTypeId = 1, ProviderType = ProviderType.Main, RemovedReasonId = null},
            new PatchOrganisationModel{ Status = OrganisationStatus.Active, OrganisationTypeId = 2, ProviderType = ProviderType.Main, RemovedReasonId = null},
        },
        new object[]
        {
            new PatchOrganisationModel{ Status = OrganisationStatus.Active, OrganisationTypeId = 1, ProviderType = ProviderType.Main, RemovedReasonId = null},
            new PatchOrganisationModel{ Status = OrganisationStatus.Active, OrganisationTypeId = 1, ProviderType = ProviderType.Employer, RemovedReasonId = null},
        },
        new object[]
        {
            new PatchOrganisationModel{ Status = OrganisationStatus.Active, OrganisationTypeId = 1, ProviderType = ProviderType.Main, RemovedReasonId = null},
            new PatchOrganisationModel{ Status = OrganisationStatus.Active, OrganisationTypeId = 1, ProviderType = ProviderType.Main, RemovedReasonId = 1},
        }
    };
}

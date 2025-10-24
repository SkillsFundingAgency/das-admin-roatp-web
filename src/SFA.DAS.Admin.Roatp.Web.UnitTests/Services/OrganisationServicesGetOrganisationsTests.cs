using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;
public class OrganisationServicesGetOrganisationsTests
{
    [Test, MoqAutoData]
    public async Task GetOrganisations_FromSession(
        [Frozen] Mock<IOuterApiClient> clientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] OrganisationService sut,
        List<OrganisationModel> organisations,
        CancellationToken cancellationToken
    )
    {
        sessionServiceMock.Setup(x => x.Get<List<OrganisationModel>>(SessionKeys.GetOrganisations)).Returns(organisations);

        var returnedOrganisations = await sut.GetOrganisations(cancellationToken);

        returnedOrganisations.Should().BeEquivalentTo(organisations);
        sessionServiceMock.Verify(x => x.Get<List<OrganisationModel>>(SessionKeys.GetOrganisations), Times.Once);
        clientMock.Verify(x => x.GetOrganisations(It.IsAny<CancellationToken>()), Times.Never);
        sessionServiceMock.Verify(x => x.Set<List<OrganisationModel>>(SessionKeys.GetOrganisations, It.IsAny<List<OrganisationModel>>()), Times.Never);
    }


    [Test, MoqAutoData]
    public async Task GetOrganisations_NotInSession_PutInSessionAndReturned(
        [Frozen] Mock<IOuterApiClient> clientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] OrganisationService sut,
        List<OrganisationModel> organisations,
        CancellationToken cancellationToken
    )
    {
        sessionServiceMock.Setup(x => x.Get<List<OrganisationModel>>(SessionKeys.GetOrganisations)).Returns((List<OrganisationModel>)null!);
        clientMock.Setup(x => x.GetOrganisations(cancellationToken)).ReturnsAsync(new GetOrganisationsResponse { Organisations = organisations });

        var returnedOrganisations = await sut.GetOrganisations(cancellationToken);

        returnedOrganisations.Should().BeEquivalentTo(organisations);
        sessionServiceMock.Verify(x => x.Get<List<OrganisationModel>>(SessionKeys.GetOrganisations), Times.Once);
        clientMock.Verify(x => x.GetOrganisations(cancellationToken), Times.Once);
        sessionServiceMock.Verify(x => x.Set<List<OrganisationModel>>(SessionKeys.GetOrganisations, organisations), Times.Once);
    }
}

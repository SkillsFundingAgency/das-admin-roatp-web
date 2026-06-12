using AutoFixture.NUnit4;
using FluentAssertions;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;
public class OrganisationTypesServiceTests
{
    [Test, MoqAutoData]
    public async Task GetOrganisationTypes_FromSession(
        [Frozen] Mock<IOuterApiClient> clientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] OrganisationTypesService sut,
        List<OrganisationTypeModel> organisationTypes,
        CancellationToken cancellationToken
    )
    {
        sessionServiceMock.Setup(x => x.Get<List<OrganisationTypeModel>>(SessionKeys.GetOrganisationTypes)).Returns(organisationTypes);

        var returnedOrganisationTypes = await sut.GetOrganisationTypes(cancellationToken);

        returnedOrganisationTypes.Should().BeEquivalentTo(organisationTypes);
        sessionServiceMock.Verify(x => x.Get<List<OrganisationTypeModel>>(SessionKeys.GetOrganisationTypes), Times.Once);
        clientMock.Verify(x => x.GetOrganisationTypes(It.IsAny<CancellationToken>()), Times.Never);
        sessionServiceMock.Verify(x => x.Set<List<OrganisationTypeModel>>(SessionKeys.GetOrganisationTypes, It.IsAny<List<OrganisationTypeModel>>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task GetOrganisationTypes_NotInSession_PutInSessionAndReturned(
        [Frozen] Mock<IOuterApiClient> clientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] OrganisationTypesService sut,
        List<OrganisationTypeModel> organisationTypes,
        CancellationToken cancellationToken
    )
    {
        sessionServiceMock.Setup(x => x.Get<List<OrganisationTypeModel>>(SessionKeys.GetOrganisationTypes)).Returns((List<OrganisationTypeModel>)null!);
        clientMock.Setup(x => x.GetOrganisationTypes(cancellationToken)).ReturnsAsync(new GetOrganisationTypesResponse { OrganisationTypes = organisationTypes });

        var returnedOrganisationTypes = await sut.GetOrganisationTypes(cancellationToken);

        returnedOrganisationTypes.Should().BeEquivalentTo(organisationTypes);
        sessionServiceMock.Verify(x => x.Get<List<OrganisationTypeModel>>(SessionKeys.GetOrganisationTypes), Times.Once);
        clientMock.Verify(x => x.GetOrganisationTypes(cancellationToken), Times.Once);
        sessionServiceMock.Verify(x => x.Set<List<OrganisationTypeModel>>(SessionKeys.GetOrganisationTypes, organisationTypes), Times.Once);
    }
}

using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.OrganisationTypeUpdateControllerTests;
public class OrganisationTypeUpdateControllerGetTests
{
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
       [Greedy] OrganisationTypeUpdateController sut,
       int ukprn,
       CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((GetOrganisationResponse)null!);

        var actual = await sut.Index(ukprn, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_BuildViewModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] OrganisationTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        List<OrganisationTypeModel> organisationTypes,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.Ukprn = ukprn;
        getOrganisationResponse.OrganisationType = organisationTypes.First().Description;
        getOrganisationResponse.OrganisationTypeId = organisationTypes.First().Id;

        foreach (var item in organisationTypes)
        {
            item.IsSelected = item.Id == getOrganisationResponse.OrganisationTypeId;
        }

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);
        outerApiClientMock.Setup(x => x.GetOrganisationTypes(It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new GetOrganisationTypesResponse { OrganisationTypes = organisationTypes });

        var actual = await sut.Index(ukprn, cancellationToken) as ViewResult;
        actual.Should().NotBeNull();
        var model = actual.Model as OrganisationTypeUpdateViewModel;
        model.Should().NotBeNull();
        model.OrganisationTypeId.Should().Be((int)getOrganisationResponse.OrganisationTypeId);
        model.OrganisationTypes.Should().BeEquivalentTo(organisationTypes);
    }
}

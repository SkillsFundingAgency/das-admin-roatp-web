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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ProviderTypeUpdateControllerTests;
public class ProviderTypeUpdateControllerGetTests
{
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderTypeUpdateController sut,
        string ukprn,
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
        [Greedy] ProviderTypeUpdateController sut,
        string selectOrganisationLink,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        ProviderType providerType,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.Ukprn = ukprn;
        getOrganisationResponse.ProviderType = providerType;
        var providerTypeId = (int)getOrganisationResponse.ProviderType;
        var expectedOrganisationTypes = BuildProviderTypes(providerTypeId);
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        var actual = await sut.Index(ukprn.ToString(), cancellationToken) as ViewResult;
        actual.Should().NotBeNull();
        var model = actual.Model as ProviderTypeUpdateViewModel;
        model.Should().NotBeNull();
        model.ProviderTypeId.Should().Be((int)getOrganisationResponse.ProviderType);
        model.ProviderTypes.Should().BeEquivalentTo(expectedOrganisationTypes);
    }

    private static List<OrganisationRouteSelectionModel> BuildProviderTypes(int providerTypeId)
    {
        return new List<OrganisationRouteSelectionModel>
        {
            new() { Description = "Main provider", Id = (int)ProviderType.Main, IsSelected = providerTypeId == (int)ProviderType.Main },
            new() { Description = "Employer provider", Id = (int)ProviderType.Employer, IsSelected = providerTypeId == (int)ProviderType.Employer },
            new() { Description = "Supporting provider", Id = (int)ProviderType.Supporting, IsSelected = providerTypeId == (int)ProviderType.Supporting },
        };
    }
}

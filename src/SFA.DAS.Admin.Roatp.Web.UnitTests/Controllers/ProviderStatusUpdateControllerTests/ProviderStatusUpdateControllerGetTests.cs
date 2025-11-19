using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ProviderStatusUpdateControllerTests;
public class ProviderStatusUpdateControllerGetTests
{
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderStatusUpdateController sut,
        int ukprn,
        CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((ApiResponse<GetOrganisationResponse>)null!);

        var actual = await sut.Index(ukprn, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_BuildViewModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderStatusUpdateController sut,
        string selectOrganisationLink,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.Ukprn = ukprn;
        var expectedOrganisationStatuses = BuildOrganisationStatuses(getOrganisationResponse.Status);
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken) as ViewResult;
        actual.Should().NotBeNull();
        var model = actual.Model as OrganisationStatusUpdateViewModel;
        model.Should().NotBeNull();
        model.OrganisationStatus.Should().Be(getOrganisationResponse.Status);
        model.OrganisationStatuses.Should().BeEquivalentTo(expectedOrganisationStatuses);
    }

    private static List<OrganisationStatusSelectionModel> BuildOrganisationStatuses(OrganisationStatus status)
    {
        return new List<OrganisationStatusSelectionModel>
        {
            new() { Description = "Active", Id = (int)OrganisationStatus.Active, IsSelected = status == OrganisationStatus.Active },
            new() { Description = "Active but not taking on apprentices", Id = (int)OrganisationStatus.ActiveNoStarts, IsSelected = status == OrganisationStatus.ActiveNoStarts },
            new() { Description = "On-boarding", Id = (int)OrganisationStatus.OnBoarding, IsSelected = status == OrganisationStatus.OnBoarding },
            new() { Description = "Removed", Id = (int)OrganisationStatus.Removed, IsSelected = status == OrganisationStatus.Removed }
        };
    }
}

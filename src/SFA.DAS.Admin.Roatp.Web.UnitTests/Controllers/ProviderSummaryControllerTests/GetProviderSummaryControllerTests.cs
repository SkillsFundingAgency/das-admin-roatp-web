using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ProviderSummaryControllerTests;
public class GetProviderSummaryControllerTests
{
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> _sessionServiceMock,
        [Frozen] EditOrganisationSessionModel _editOrganisationSessionModel,
        [Greedy] ProviderSummaryController sut,
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
        _sessionServiceMock.Verify(c => c.Set<EditOrganisationSessionModel>(SessionKeys.EditOrganisation, It.IsAny<EditOrganisationSessionModel>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_SetSessionAndRedirect(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> _sessionServiceMock,
        [Frozen] EditOrganisationSessionModel _editOrganisationSessionModel,
        [Greedy] ProviderSummaryController sut,
        string selectOrganisationLink,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.Ukprn = ukprn;
        _editOrganisationSessionModel.Ukprn = ukprn;
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.SelectProvider, selectOrganisationLink);

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        var actual = await sut.Index(_editOrganisationSessionModel.Ukprn.ToString(), cancellationToken) as ViewResult;
        actual.Should().NotBeNull();
        var model = actual.Model as ProviderSummaryViewModel;
        model.Should().NotBeNull();
        model.Ukprn.Should().Be(ukprn);
        model.SearchProviderUrl.Should().Be(selectOrganisationLink);
        _sessionServiceMock.Verify(c => c.Set<EditOrganisationSessionModel>(SessionKeys.EditOrganisation, It.IsAny<EditOrganisationSessionModel>()), Times.Once);
    }
}

using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ProviderStatusConfirmationControllerTests;
public class ProviderStatusConfirmationControllerGetTests
{
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderStatusConfirmationController sut,
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
    public async Task Get_MatchingDetails_UkprnNoMatch_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderStatusConfirmationController sut,
        GetOrganisationResponse getOrganisationResponse,
        string ukprn,
        CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        var actual = await sut.Index(ukprn, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test]
    [MoqInlineAutoData(OrganisationStatus.Active)]
    [MoqInlineAutoData(OrganisationStatus.ActiveNoStarts)]
    [MoqInlineAutoData(OrganisationStatus.OnBoarding)]
    [MoqInlineAutoData(OrganisationStatus.Removed)]
    public async Task Get_MatchingDetails_BuildExpectedViewModel(
        OrganisationStatus status,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderStatusConfirmationController sut,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        string providerSummaryLink,
        string selectTrainingProviderLink,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.Ukprn = ukprn;
        getOrganisationResponse.Status = status;

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderSummary, providerSummaryLink)
            .AddUrlForRoute(RouteNames.SelectProvider, selectTrainingProviderLink);

        var expectedViewModel = new ProviderStatusConfirmationViewModel
        {
            LegalName = getOrganisationResponse.LegalName,
            OrganisationStatus = getOrganisationResponse.Status,
            Ukprn = ukprn.ToString(),
            StatusText = MatchingStatusText(getOrganisationResponse.Status),
            ProviderSummaryLink = providerSummaryLink,
            SelectTrainingProviderLink = selectTrainingProviderLink
        };

        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        var actual = await sut.Index(ukprn.ToString(), cancellationToken) as ViewResult;

        actual.Should().NotBeNull();
        var model = actual!.Model as ProviderStatusConfirmationViewModel;
        model.Should().NotBeNull();
        model.Should().BeEquivalentTo(expectedViewModel);
    }

    private static string MatchingStatusText(OrganisationStatus status)
    {
        return status switch
        {
            OrganisationStatus.Active => "active",
            OrganisationStatus.ActiveNoStarts => "active but not taking on apprentices",
            OrganisationStatus.OnBoarding => "on-boarding",
            OrganisationStatus.Removed => "removed",
            _ => ""
        };
    }
}

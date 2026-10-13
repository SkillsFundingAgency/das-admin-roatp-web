using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.RestrictProviderControllerTests;

[TestFixture]
public class RestrictProviderControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string ProviderName = "Denton Business Services Limited";

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictProvider_ThenRefreshesPage(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictProviderController sut)
    {
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);

        var result = await sut.IndexPost(Ukprn, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.RestrictProvider);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictProvider_AndProviderNameIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictProviderController sut)
    {
        sessionServiceMock.SetupProviderNameMissing();
        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<GetOrganisationResponse>(HttpStatusCode.NotFound, includeException: false));

        var result = await sut.IndexPost(Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}

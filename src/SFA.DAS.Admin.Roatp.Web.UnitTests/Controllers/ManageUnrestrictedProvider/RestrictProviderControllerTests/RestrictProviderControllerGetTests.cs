using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.RestrictProviderControllerTests;

[TestFixture]
public class RestrictProviderControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string ProviderName = "Denton Business Services Limited";
    private const string ProviderSummaryUrl = "/providers/10019900";

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictProvider_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictProviderController sut)
    {
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderSummary, ProviderSummaryUrl);

        var result = await sut.Index(Ukprn, CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictProviderViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RestrictProviderController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.ProviderName.Should().Be(ProviderName);
            model.CancelUrl.Should().Be(ProviderSummaryUrl);
            model.BackLinkUrl.Should().Be(ProviderSummaryUrl);
            model.BackLinkText.Should().Be(RestrictProviderViewModel.BackLinkTextValue);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictProvider_AndProviderNameIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictProviderController sut)
    {
        sessionServiceMock.SetupProviderNameMissing();
        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<GetOrganisationResponse>(HttpStatusCode.NotFound, includeException: false));

        var result = await sut.Index(Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}

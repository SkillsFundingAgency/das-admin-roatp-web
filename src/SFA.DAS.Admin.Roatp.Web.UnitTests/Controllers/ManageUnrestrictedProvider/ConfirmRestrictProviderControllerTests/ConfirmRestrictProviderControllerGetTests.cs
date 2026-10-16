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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ConfirmRestrictProviderControllerTests;

[TestFixture]
public class ConfirmRestrictProviderControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string ProviderName = "Denton Business Services Limited";
    private const string ProviderSummaryUrl = "/providers/10019900";

    [Test, MoqAutoData]
    public async Task WhenGettingConfirm_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ConfirmRestrictProviderController sut)
    {
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderSummary, ProviderSummaryUrl);

        var result = await sut.Index(Ukprn, CancellationToken.None) as ViewResult;
        var model = result?.Model as ConfirmRestrictProviderViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ConfirmRestrictProviderController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.ProviderName.Should().Be(ProviderName);
            model.CancelUrl.Should().Be(ProviderSummaryUrl);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingConfirm_AndProviderNameIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ConfirmRestrictProviderController sut)
    {
        sessionServiceMock.SetupProviderNameMissing();
        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<GetOrganisationResponse>(HttpStatusCode.NotFound, includeException: false));

        var result = await sut.Index(Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}

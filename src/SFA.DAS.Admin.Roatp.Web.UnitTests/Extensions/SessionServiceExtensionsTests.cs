using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;

[TestFixture]
public class SessionServiceExtensionsTests
{
    private const int Ukprn = 10019900;
    private const string ProviderName = "Denton Business Services Limited";

    [Test]
    public void WhenGettingProviderName_AndNameIsInSession_ThenReturnsSessionName()
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);

        var result = sessionServiceMock.Object.GetProviderNameSession(Ukprn);

        result.Should().Be(ProviderName);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingProviderName_AndNameIsInSession_ThenDoesNotCallOrganisation(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);

        var result = await sessionServiceMock.Object.GetProviderName(
            outerApiClientMock.Object,
            Ukprn,
            CancellationToken.None);

        result.Should().Be(ProviderName);
        outerApiClientMock.Verify(c => c.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingProviderName_AndNameIsNotInSession_ThenLoadsNameFromOrganisationAndStoresIt(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        GetOrganisationResponse organisationResponse)
    {
        organisationResponse.Ukprn = Ukprn;
        organisationResponse.LegalName = ProviderName;
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.SetupProviderNameMissing(Ukprn);
        SetupOrganisation(outerApiClientMock, organisationResponse, HttpStatusCode.OK);

        var result = await sessionServiceMock.Object.GetProviderName(
            outerApiClientMock.Object,
            Ukprn,
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(ProviderName);
        }

        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.ProviderName(Ukprn),
            It.Is<ProviderNameSessionModel>(m =>
                m.Ukprn == Ukprn &&
                m.ProviderName == ProviderName)), Times.Once);
        outerApiClientMock.Verify(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingProviderName_AndOrganisationIsNotFound_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        GetOrganisationResponse organisationResponse)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.SetupProviderNameMissing(Ukprn);
        SetupOrganisation(outerApiClientMock, organisationResponse, HttpStatusCode.NotFound);

        var result = await sessionServiceMock.Object.GetProviderName(
            outerApiClientMock.Object,
            Ukprn,
            CancellationToken.None);

        result.Should().BeNull();
        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderName(Ukprn), It.IsAny<ProviderNameSessionModel>()),
            Times.Never);
    }

    private static void SetupOrganisation(
        Mock<IOuterApiClient> outerApiClientMock,
        GetOrganisationResponse response,
        HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(statusCode),
                response,
                new RefitSettings(),
                null));
    }
}

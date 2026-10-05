using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;

[TestFixture]
public class SessionServiceExtensionsTests
{
    private const int Ukprn = 10019900;
    private const string ProviderName = "Denton Business Services Limited";

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
        sessionServiceMock.SetupProviderNameMissing();
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
            SessionKeys.ProviderNames,
            It.Is<Dictionary<int, string>>(d => d[Ukprn] == ProviderName)), Times.Once);
        outerApiClientMock.Verify(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingProviderName_AndOrganisationIsNotFound_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        GetOrganisationResponse organisationResponse)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.SetupProviderNameMissing();
        SetupOrganisation(outerApiClientMock, organisationResponse, HttpStatusCode.NotFound);

        var result = await sessionServiceMock.Object.GetProviderName(
            outerApiClientMock.Object,
            Ukprn,
            CancellationToken.None);

        result.Should().BeNull();
        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderNames, It.IsAny<Dictionary<int, string>>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingProviderName_AndOrganisationReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        GetOrganisationResponse organisationResponse)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.SetupProviderNameMissing();
        SetupOrganisation(outerApiClientMock, organisationResponse, HttpStatusCode.InternalServerError);

        var act = () => sessionServiceMock.Object.GetProviderName(
            outerApiClientMock.Object,
            Ukprn,
            CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderNames, It.IsAny<Dictionary<int, string>>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingProviderName_AndOrganisationLegalNameIsMissing_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        GetOrganisationResponse organisationResponse)
    {
        organisationResponse.LegalName = " ";
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.SetupProviderNameMissing();
        SetupOrganisation(outerApiClientMock, organisationResponse, HttpStatusCode.OK);

        var result = await sessionServiceMock.Object.GetProviderName(
            outerApiClientMock.Object,
            Ukprn,
            CancellationToken.None);

        result.Should().BeNull();
        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderNames, It.IsAny<Dictionary<int, string>>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingProviderName_AndSessionHasADifferentUkprn_ThenLoadsNameFromOrganisationAndMergesIt(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        GetOrganisationResponse organisationResponse)
    {
        const int otherUkprn = 10000001;
        const string otherProviderName = "Other provider";
        organisationResponse.Ukprn = Ukprn;
        organisationResponse.LegalName = ProviderName;
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.SetupProviderName(otherUkprn, otherProviderName);
        SetupOrganisation(outerApiClientMock, organisationResponse, HttpStatusCode.OK);

        var result = await sessionServiceMock.Object.GetProviderName(
            outerApiClientMock.Object,
            Ukprn,
            CancellationToken.None);

        result.Should().Be(ProviderName);
        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.ProviderNames,
            It.Is<Dictionary<int, string>>(d =>
                d[otherUkprn] == otherProviderName && d[Ukprn] == ProviderName)), Times.Once);
        outerApiClientMock.Verify(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SetupOrganisation(
        Mock<IOuterApiClient> outerApiClientMock,
        GetOrganisationResponse response,
        HttpStatusCode statusCode)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        ApiException? apiException = null;
        if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NotFound)
        {
            apiException = ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Get,
                httpResponse,
                new RefitSettings()).GetAwaiter().GetResult();
        }

        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                httpResponse,
                response,
                new RefitSettings(),
                apiException));
    }
}

using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class UkprnServiceTests
{
    [Test, MoqAutoData]
    public async Task WhenGettingOrganisation_AndApiReturnsOrganisation_ThenReturnsContent(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
        int ukprn,
        GetOrganisationResponse response)
    {
        var sut = CreateSut(outerApiClientMock, httpContextAccessorMock);
        outerApiClientMock
            .Setup(c => c.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));

        var result = await sut.GetOrganisationAsync(ukprn, CancellationToken.None);

        result.Should().Be(response);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingOrganisation_AndApiReturnsNotFound_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
        int ukprn)
    {
        var sut = CreateSut(outerApiClientMock, httpContextAccessorMock);
        outerApiClientMock
            .Setup(c => c.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound), null, new RefitSettings(), null));

        var result = await sut.GetOrganisationAsync(ukprn, CancellationToken.None);

        result.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingOrganisation_AndApiReturnsNonSuccessNonNotFound_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
        int ukprn)
    {
        var sut = CreateSut(outerApiClientMock, httpContextAccessorMock);
        outerApiClientMock
            .Setup(c => c.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(HttpStatusCode.InternalServerError), null, new RefitSettings(), null));

        var act = () => sut.GetOrganisationAsync(ukprn, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingOrganisation_AndCalledTwice_ThenCallsApiOnce(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
        int ukprn,
        GetOrganisationResponse response)
    {
        var sut = CreateSut(outerApiClientMock, httpContextAccessorMock);
        outerApiClientMock
            .Setup(c => c.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));

        var first = await sut.GetOrganisationAsync(ukprn, CancellationToken.None);
        var second = await sut.GetOrganisationAsync(ukprn, CancellationToken.None);

        first.Should().Be(response);
        second.Should().Be(response);
        outerApiClientMock.Verify(c => c.GetOrganisation(ukprn, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingOrganisation_AndHttpContextIsMissing_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
        int ukprn)
    {
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        var sut = new UkprnService(outerApiClientMock.Object, new CacheService(httpContextAccessorMock.Object));

        var act = () => sut.GetOrganisationAsync(ukprn, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("HttpContext is not available.");
    }

    private static UkprnService CreateSut(
        Mock<IOuterApiClient> outerApiClientMock,
        Mock<IHttpContextAccessor> httpContextAccessorMock)
    {
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        return new UkprnService(outerApiClientMock.Object, new CacheService(httpContextAccessorMock.Object));
    }
}

using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class NotAllowedProvidersServiceTests
{
    [Test, MoqAutoData]
    public async Task WhenGettingNotAllowedProviders_AndApiReturnsCourse_ThenCachesAndReturnsContent(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] NotAllowedProvidersService sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response)
    {
        var cacheKey = SessionKeys.NotAllowedProvidersForRestrictedCourse(larsCode);
        sessionServiceMock.Setup(s => s.Contains(cacheKey)).Returns(false);
        outerApiClientMock
            .Setup(c => c.GetNotAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));

        var result = await sut.GetNotAllowedProvidersAsync(larsCode, CancellationToken.None);

        result.Should().Be(response);
        sessionServiceMock.Verify(s => s.Set(cacheKey, response), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingNotAllowedProviders_AndCached_ThenDoesNotCallApi(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] NotAllowedProvidersService sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response)
    {
        var cacheKey = SessionKeys.NotAllowedProvidersForRestrictedCourse(larsCode);
        sessionServiceMock.Setup(s => s.Contains(cacheKey)).Returns(true);
        sessionServiceMock.Setup(s => s.Get<GetRestrictedCourseDetailsResponse>(cacheKey)).Returns(response);

        var result = await sut.GetNotAllowedProvidersAsync(larsCode, CancellationToken.None);

        result.Should().Be(response);
        outerApiClientMock.Verify(
            c => c.GetNotAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        sessionServiceMock.Verify(
            s => s.Set(It.IsAny<string>(), It.IsAny<GetRestrictedCourseDetailsResponse>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingNotAllowedProviders_AndApiReturnsNotFound_ThenCachesNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] NotAllowedProvidersService sut,
        string larsCode)
    {
        var cacheKey = SessionKeys.NotAllowedProvidersForRestrictedCourse(larsCode);
        sessionServiceMock.Setup(s => s.Contains(cacheKey)).Returns(false);
        outerApiClientMock
            .Setup(c => c.GetNotAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound), null, new RefitSettings(), null));

        var result = await sut.GetNotAllowedProvidersAsync(larsCode, CancellationToken.None);

        result.Should().BeNull();
        sessionServiceMock.Verify(
            s => s.Set(cacheKey, (GetRestrictedCourseDetailsResponse?)null),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingNotAllowedProviders_AndApiReturnsNonSuccessNonNotFound_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] NotAllowedProvidersService sut,
        string larsCode)
    {
        var cacheKey = SessionKeys.NotAllowedProvidersForRestrictedCourse(larsCode);
        sessionServiceMock.Setup(s => s.Contains(cacheKey)).Returns(false);
        outerApiClientMock
            .Setup(c => c.GetNotAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.InternalServerError), null, new RefitSettings(), null));

        var act = () => sut.GetNotAllowedProvidersAsync(larsCode, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
        sessionServiceMock.Verify(
            s => s.Set(It.IsAny<string>(), It.IsAny<GetRestrictedCourseDetailsResponse>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingNotAllowedProviders_AndCalledTwice_ThenCallsApiOnce(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] NotAllowedProvidersService sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response)
    {
        var cacheKey = SessionKeys.NotAllowedProvidersForRestrictedCourse(larsCode);
        var cached = false;
        sessionServiceMock
            .Setup(s => s.Contains(cacheKey))
            .Returns(() => cached);
        sessionServiceMock
            .Setup(s => s.Set(cacheKey, response))
            .Callback(() => cached = true);
        sessionServiceMock
            .Setup(s => s.Get<GetRestrictedCourseDetailsResponse>(cacheKey))
            .Returns(response);
        outerApiClientMock
            .Setup(c => c.GetNotAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));

        var first = await sut.GetNotAllowedProvidersAsync(larsCode, CancellationToken.None);
        var second = await sut.GetNotAllowedProvidersAsync(larsCode, CancellationToken.None);

        first.Should().Be(response);
        second.Should().Be(response);
        outerApiClientMock.Verify(
            c => c.GetNotAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

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
public class LarsCodeServiceTests
{
    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndApiReturnsCourse_ThenReturnsContent(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
        [Greedy] LarsCodeService sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response)
    {
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));

        var result = await sut.GetCourseDetailsAsync(larsCode, CancellationToken.None);

        result.Should().Be(response);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndApiFails_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
        [Greedy] LarsCodeService sut,
        string larsCode)
    {
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound), null, new RefitSettings(), null));

        var result = await sut.GetCourseDetailsAsync(larsCode, CancellationToken.None);

        result.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndApiReturnsOkWithNullContent_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
        [Greedy] LarsCodeService sut,
        string larsCode)
    {
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), null, new RefitSettings(), null));

        var result = await sut.GetCourseDetailsAsync(larsCode, CancellationToken.None);

        result.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndApiReturnsNonSuccessNonNotFound_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
        [Greedy] LarsCodeService sut,
        string larsCode)
    {
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.InternalServerError), null, new RefitSettings(), null));

        var act = () => sut.GetCourseDetailsAsync(larsCode, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndCalledTwice_ThenCallsApiOnce(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
        [Greedy] LarsCodeService sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response)
    {
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));

        var first = await sut.GetCourseDetailsAsync(larsCode, CancellationToken.None);
        var second = await sut.GetCourseDetailsAsync(larsCode, CancellationToken.None);

        first.Should().Be(response);
        second.Should().Be(response);
        outerApiClientMock.Verify(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndHttpContextIsMissing_ThenThrows(
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
        [Greedy] LarsCodeService sut,
        string larsCode)
    {
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var act = () => sut.GetCourseDetailsAsync(larsCode, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("HttpContext is not available.");
    }
}

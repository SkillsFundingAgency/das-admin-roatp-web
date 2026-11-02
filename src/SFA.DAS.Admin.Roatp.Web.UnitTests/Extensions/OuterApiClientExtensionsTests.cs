using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;

[TestFixture]
public class OuterApiClientExtensionsTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";

    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndApiReturnsOk_ThenReturnsContent(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        GetRestrictedCourseDetailsResponse courseDetails)
    {
        SetupAllowedProvidersForCourse(outerApiClientMock, courseDetails, HttpStatusCode.OK);

        var result = await outerApiClientMock.Object.GetCourseDetails(LarsCode, new CancellationToken());

        result.Should().Be(courseDetails);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndApiReturnsNotFound_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        GetRestrictedCourseDetailsResponse courseDetails)
    {
        SetupAllowedProvidersForCourse(outerApiClientMock, courseDetails, HttpStatusCode.NotFound);

        var result = await outerApiClientMock.Object.GetCourseDetails(LarsCode, new CancellationToken());

        result.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        GetRestrictedCourseDetailsResponse courseDetails)
    {
        SetupAllowedProvidersForCourse(outerApiClientMock, courseDetails, HttpStatusCode.InternalServerError);

        var act = () => outerApiClientMock.Object.GetCourseDetails(LarsCode, new CancellationToken());

        await act.Should().ThrowAsync<ApiException>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourse_AndApiReturnsOk_ThenReturnsContent(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        ProviderAllowedCourseModel allowedCourse)
    {
        SetupProviderAllowedCourse(outerApiClientMock, allowedCourse, HttpStatusCode.OK);

        var result = await outerApiClientMock.Object.GetAllowedCourse(Ukprn, LarsCode, new CancellationToken());

        result.Should().Be(allowedCourse);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourse_AndApiReturnsNotFound_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        ProviderAllowedCourseModel allowedCourse)
    {
        SetupProviderAllowedCourse(outerApiClientMock, allowedCourse, HttpStatusCode.NotFound);

        var result = await outerApiClientMock.Object.GetAllowedCourse(Ukprn, LarsCode, new CancellationToken());

        result.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourse_AndApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        ProviderAllowedCourseModel allowedCourse)
    {
        SetupProviderAllowedCourse(outerApiClientMock, allowedCourse, HttpStatusCode.InternalServerError);

        var act = () => outerApiClientMock.Object.GetAllowedCourse(Ukprn, LarsCode, new CancellationToken());

        await act.Should().ThrowAsync<ApiException>();
    }

    private static void SetupAllowedProvidersForCourse(
        Mock<IOuterApiClient> outerApiClientMock,
        GetRestrictedCourseDetailsResponse response,
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
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                httpResponse,
                response,
                new RefitSettings(),
                apiException));
    }

    private static void SetupProviderAllowedCourse(
        Mock<IOuterApiClient> outerApiClientMock,
        ProviderAllowedCourseModel response,
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
            .Setup(c => c.GetProviderAllowedCourse(Ukprn, LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<ProviderAllowedCourseModel>(
                httpResponse,
                response,
                new RefitSettings(),
                apiException));
    }
}

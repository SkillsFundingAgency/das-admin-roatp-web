using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class CoursesServiceTests
{
    private const string LarsCode = "105";
    private static readonly DateTime LastDateStarts = new(2028, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenGettingCourse_AndCoursesAreCached_ThenReturnsCourseWithoutCallingApi(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Greedy] CoursesService sut)
    {
        SetupCachedCourses(applicationCacheMock);

        var result = await sut.GetCourse(LarsCode, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.LarsCode.Should().Be(LarsCode);
            result.LastDateStarts.Should().Be(LastDateStarts);
        }

        outerApiClientMock.Verify(c => c.GetCourses(It.IsAny<CancellationToken>()), Times.Never);
        applicationCacheMock.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<GetCoursesResponse>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourse_AndCoursesAreNotCached_ThenCallsApiCachesAndReturnsCourse(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Greedy] CoursesService sut)
    {
        SetupCacheMiss(applicationCacheMock);
        SetupGetCoursesApi(outerApiClientMock);

        var result = await sut.GetCourse(LarsCode, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.LarsCode.Should().Be(LarsCode);
            result.LastDateStarts.Should().Be(LastDateStarts);
        }

        outerApiClientMock.Verify(c => c.GetCourses(It.IsAny<CancellationToken>()), Times.Once);
        applicationCacheMock.Verify(
            c => c.SetAsync(
                ApplicationCacheKeys.CoursesCacheKey,
                It.Is<GetCoursesResponse>(r => r.Courses.Any(course => course.LarsCode == LarsCode)),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourse_AndCourseIsNotInCoursesList_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Greedy] CoursesService sut)
    {
        SetupCacheMiss(applicationCacheMock);
        SetupGetCoursesApi(outerApiClientMock, includeRequestedCourse: false);

        var result = await sut.GetCourse(LarsCode, CancellationToken.None);

        result.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourse_AndApiReturnsNotFound_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Greedy] CoursesService sut)
    {
        SetupCacheMiss(applicationCacheMock);
        SetupGetCoursesApi(outerApiClientMock, HttpStatusCode.NotFound);

        var act = () => sut.GetCourse(LarsCode, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
        applicationCacheMock.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<GetCoursesResponse>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourse_AndApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Greedy] CoursesService sut)
    {
        SetupCacheMiss(applicationCacheMock);
        SetupGetCoursesApi(outerApiClientMock, HttpStatusCode.InternalServerError);

        var act = () => sut.GetCourse(LarsCode, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourse_AndApiContentIsNull_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Greedy] CoursesService sut)
    {
        SetupCacheMiss(applicationCacheMock);
        outerApiClientMock
            .Setup(c => c.GetCourses(It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<GetCoursesResponse>(HttpStatusCode.OK, content: null));

        var result = await sut.GetCourse(LarsCode, CancellationToken.None);

        result.Should().BeNull();
        applicationCacheMock.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<GetCoursesResponse>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static void SetupCachedCourses(Mock<IApplicationCacheService> applicationCacheMock)
    {
        applicationCacheMock
            .Setup(c => c.GetAsync<GetCoursesResponse>(
                ApplicationCacheKeys.CoursesCacheKey,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCoursesResponse { Courses = [CreateCourse()] });
    }

    private static void SetupCacheMiss(Mock<IApplicationCacheService> applicationCacheMock)
    {
        applicationCacheMock
            .Setup(c => c.GetAsync<GetCoursesResponse>(
                ApplicationCacheKeys.CoursesCacheKey,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCoursesResponse?)null);
        applicationCacheMock
            .Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<GetCoursesResponse>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static void SetupGetCoursesApi(
        Mock<IOuterApiClient> outerApiClientMock,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        bool includeRequestedCourse = true)
    {
        var courses = new List<GetCourseResponse>
        {
            new()
            {
                LarsCode = "300",
                Title = "Beta course",
                Level = 4,
                CourseType = CourseType.Apprenticeship,
                LearningType = LearningType.Apprenticeship
            }
        };

        if (includeRequestedCourse)
        {
            courses.Insert(0, CreateCourse());
        }

        outerApiClientMock
            .Setup(c => c.GetCourses(It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create(
                statusCode,
                new GetCoursesResponse { Courses = courses }));
    }

    private static GetCourseResponse CreateCourse() => new()
    {
        LarsCode = LarsCode,
        Title = "Electrical",
        Level = 3,
        CourseType = CourseType.Apprenticeship,
        LearningType = LearningType.Apprenticeship,
        LastDateStarts = LastDateStarts
    };
}

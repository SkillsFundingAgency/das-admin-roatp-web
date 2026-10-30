using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageRestrictedProvider.RemoveAllowedCourseLastStartDateControllerTests;

[TestFixture]
public class RemoveAllowedCourseLastStartDateControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string CourseTitle = "Carpentry";
    private const int CourseLevel = 1;
    private const string UnitCourseTitle = "Permanent modular building assembly - Apprenticeship unit";
    private const int UnitCourseLevel = 4;

    [Test, MoqAutoData]
    public async Task WhenPostingRemoveLastStartDate_ThenRemovesLastStartDateSetsBannerAndRedirectsToList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupPutResponse(outerApiClientMock, HttpStatusCode.OK);
        sut.SetupAuthenticatedUser();
        sut.AddTempData();

        var result = await sut.IndexPost(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new CancellationToken()) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderAllowedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["routeLearningType"].Should().Be(RouteLearningType.Apprenticeship);
            sut.TempData[ProviderAllowedCoursesController.SuccessBannerTempDataKey]
                .Should().Be(RemoveAllowedCourseLastStartDateController.GetSuccessBannerMessage("Carpentry (Level 1)"));
        }

        outerApiClientMock.Verify(
            c => c.GetProviderAllowedCourse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(c => c.PutProviderAllowedCourse(
            Ukprn,
            LarsCode,
            It.Is<PutProviderAllowedCourseRequest>(r =>
                r.LastDateStarts == null
                && r.UserId == sut.User.UserId()
                && r.UserDisplayName == sut.User.UserDisplayName()),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRemoveLastStartDate_AndRouteLearningTypeIsApprenticeshipUnit_ThenRemovesLastStartDateSetsBannerAndRedirectsToList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupGetCourse(coursesServiceMock, CourseType.ShortCourse, UnitCourseTitle, UnitCourseLevel);
        SetupPutResponse(outerApiClientMock, HttpStatusCode.OK);
        sut.SetupAuthenticatedUser();
        sut.AddTempData();

        var result = await sut.IndexPost(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            LarsCode,
            new CancellationToken()) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderAllowedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["routeLearningType"].Should().Be(RouteLearningType.ApprenticeshipUnit);
            sut.TempData[ProviderAllowedCoursesController.SuccessBannerTempDataKey]
                .Should().Be(RemoveAllowedCourseLastStartDateController.GetSuccessBannerMessage(
                    "Permanent modular building assembly - Apprenticeship unit (Level 4)"));
        }

        outerApiClientMock.Verify(c => c.PutProviderAllowedCourse(
            Ukprn,
            LarsCode,
            It.Is<PutProviderAllowedCourseRequest>(r =>
                r.LastDateStarts == null
                && r.UserId == sut.User.UserId()
                && r.UserDisplayName == sut.User.UserDisplayName()),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRemoveLastStartDate_AndCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCourseResponse?)null);

        var result = await sut.IndexPost(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
        outerApiClientMock.Verify(
            c => c.PutProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<PutProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRemoveLastStartDate_AndPutReturnsNotFound_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupPutResponse(outerApiClientMock, HttpStatusCode.NotFound);
        sut.SetupAuthenticatedUser();

        var act = () => sut.IndexPost(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new CancellationToken());

        await act.Should().ThrowAsync<ApiException>();
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRemoveLastStartDate_AndPutReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupPutResponse(outerApiClientMock, HttpStatusCode.InternalServerError);
        sut.SetupAuthenticatedUser();

        var act = () => sut.IndexPost(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new CancellationToken());

        await act.Should().ThrowAsync<ApiException>();
    }

    private static void SetupGetCourse(
        Mock<ICoursesService> coursesServiceMock,
        CourseType courseType = CourseType.Apprenticeship,
        string title = CourseTitle,
        int level = CourseLevel)
    {
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCourseResponse
            {
                LarsCode = LarsCode,
                Title = title,
                Level = level,
                CourseType = courseType
            });
    }

    private static void SetupPutResponse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.PutProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<PutProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<object>(
                statusCode,
                method: HttpMethod.Put));
    }
}

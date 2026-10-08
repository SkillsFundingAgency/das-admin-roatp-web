using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ConfirmProviderRestrictedCourseControllerTests;

public class ConfirmProviderRestrictedCourseControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string DisplayTitle = "Carpentry (Level 1)";

    [Test, MoqAutoData]
    public async Task WhenPostingConfirm_ThenRestrictsCourseSetsBannerAndRedirectsToList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut)
    {
        SetupSession(sessionServiceMock);
        sut.SetupAuthenticatedUser();
        sut.AddTempData();
        SetupUpsertResponse(outerApiClientMock, HttpStatusCode.OK);

        var result = await sut.Index(Ukprn, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            sut.TempData[ProviderRestrictedApprenticeshipsController.SuccessBannerTempDataKey]
                .Should().Be(ConfirmProviderRestrictedCourseController.GetSuccessBannerMessage(DisplayTitle));
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.ProviderRestrictedCourse), Times.Once);
        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            Ukprn,
            LarsCode,
            It.Is<UpsertProviderAllowedCourseRequest>(r =>
                r.UserId == MockedUser.AuthenticatedUser.UserId()
                && r.UserDisplayName == MockedUser.AuthenticatedUser.UserDisplayName()
                && r.LastDateStarts == null
                && r.IsStartRestricted),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingConfirm_AndSessionIsMissing_ThenRedirectsWithoutCallingApi(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<ProviderRestrictedCourseSessionModel>(SessionKeys.ProviderRestrictedCourse))
            .Returns((ProviderRestrictedCourseSessionModel?)null);

        var result = await sut.Index(Ukprn, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<UpsertProviderAllowedCourseRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.ProviderRestrictedCourse), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingConfirm_AndApiReturnsNotFound_ThenThrows(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut)
    {
        SetupSession(sessionServiceMock);
        sut.SetupAuthenticatedUser();
        SetupUpsertResponse(outerApiClientMock, HttpStatusCode.NotFound);

        var act = () => sut.Index(Ukprn, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.ProviderRestrictedCourse), Times.Never);
    }

    private static void SetupSession(Mock<ISessionService> sessionServiceMock)
    {
        sessionServiceMock
            .Setup(s => s.Get<ProviderRestrictedCourseSessionModel>(SessionKeys.ProviderRestrictedCourse))
            .Returns(new ProviderRestrictedCourseSessionModel
            {
                Ukprn = Ukprn,
                LarsCode = LarsCode,
                CourseDisplayTitle = DisplayTitle,
                ProviderName = "Denton Business Services Limited"
            });
    }

    private static void SetupUpsertResponse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.UpsertProviderAllowedCourse(
                Ukprn,
                LarsCode,
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<object>(statusCode, method: HttpMethod.Post));
    }
}

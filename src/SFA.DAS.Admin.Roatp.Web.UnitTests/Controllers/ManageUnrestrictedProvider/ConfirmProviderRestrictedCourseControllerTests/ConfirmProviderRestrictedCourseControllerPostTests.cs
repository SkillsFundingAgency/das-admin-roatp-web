using System.Net;
using System.Security.Claims;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ConfirmProviderRestrictedCourseControllerTests;

public class ConfirmProviderRestrictedCourseControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string DisplayTitle = "Carpentry (Level 1)";

    [Test, MoqAutoData]
    public async Task WhenPostingConfirm_ThenRestrictsCourseSetsBanner_AndRedirectsToList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut)
    {
        SetupSession(sessionServiceMock);
        SetupAuthenticatedUser(sut);
        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
        SetupUpsertResponse(outerApiClientMock, HttpStatusCode.OK);

        var result = await sut.Index(Ukprn, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            sut.TempData[RestrictedApprenticeshipsController.SuccessBannerTempDataKey]
                .Should().Be(ConfirmProviderRestrictedCourseController.GetSuccessBannerMessage(DisplayTitle));
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.ProviderRestrictedCourse), Times.Once);
        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            Ukprn,
            LarsCode,
            It.Is<UpsertProviderAllowedCourseRequest>(r =>
                r.UserId == "TestUser@education.gov.uk"
                && r.UserDisplayName == "Test User"
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
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
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
    public async Task WhenPostingConfirm_AndApiReturnsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut)
    {
        SetupSession(sessionServiceMock);
        SetupAuthenticatedUser(sut);
        SetupUpsertResponse(outerApiClientMock, HttpStatusCode.NotFound);

        var result = await sut.Index(Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();

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
                Title = "Carpentry",
                Level = 1,
                CourseDisplayTitle = DisplayTitle
            });
    }

    private static void SetupAuthenticatedUser(ConfirmProviderRestrictedCourseController sut)
    {
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn", "TestUser@education.gov.uk")
                ], "test"))
            }
        };
    }

    private static void SetupUpsertResponse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.UpsertProviderAllowedCourse(
                Ukprn,
                LarsCode,
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<object>(
                new HttpResponseMessage(statusCode),
                null,
                new RefitSettings(),
                null));
    }
}

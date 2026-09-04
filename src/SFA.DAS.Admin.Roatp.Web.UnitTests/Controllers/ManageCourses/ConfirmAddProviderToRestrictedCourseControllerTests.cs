using System.Net;
using System.Security.Claims;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class ConfirmAddProviderToRestrictedCourseControllerTests
{
    private const string LarsCode = "105";
    private const int Ukprn = 10007938;
    private const string LegalName = "TEC PARTNERSHIP";

    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";

    [Test, MoqAutoData]
    public void WhenGettingConfirm_AndSessionExists_ThenReturnsViewWithProviderDetails(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmAddProviderToRestrictedCourseController sut)
    {
        var session = new AddProviderToRestrictedCourseSessionModel
        {
            LarsCode = LarsCode,
            CourseDisplayTitle = "Academic professional (Level 7)",
            Ukprn = Ukprn,
            LegalName = LegalName
        };

        sessionServiceMock
            .Setup(s => s.Get<AddProviderToRestrictedCourseSessionModel>(SessionKeys.AddProviderToRestrictedCourse))
            .Returns(session);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = sut.Index(LarsCode) as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be(ConfirmAddProviderToRestrictedCourseController.ViewPath);

        var model = result.Model as ConfirmAddProviderToRestrictedCourseViewModel;
        model.Should().NotBeNull();
        model!.LarsCode.Should().Be(LarsCode);
        model.ProviderName.Should().Be(LegalName.ToUpperInvariant());
        model.Ukprn.Should().Be(Ukprn);
        model.CancelUrl.Should().Be(RestrictedCourseDetailsUrl);
    }

    [Test, MoqAutoData]
    public void WhenGettingConfirm_AndSessionMissing_ThenRedirectsToRestrictedCourseDetails(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ConfirmAddProviderToRestrictedCourseController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<AddProviderToRestrictedCourseSessionModel>(SessionKeys.AddProviderToRestrictedCourse))
            .Returns((AddProviderToRestrictedCourseSessionModel?)null);

        var result = sut.Index(LarsCode) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.RestrictedCourseDetails);
        result.RouteValues!["larsCode"].Should().Be(LarsCode);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingConfirm_ThenUpsertsProviderAllowedCourseDeletesSessionSetsBannerAndRedirects(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmAddProviderToRestrictedCourseController sut)
    {
        var session = new AddProviderToRestrictedCourseSessionModel
        {
            LarsCode = LarsCode,
            CourseDisplayTitle = "Academic professional (Level 7)",
            Ukprn = Ukprn,
            LegalName = LegalName
        };

        sessionServiceMock
            .Setup(s => s.Get<AddProviderToRestrictedCourseSessionModel>(SessionKeys.AddProviderToRestrictedCourse))
            .Returns(session);

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

        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

        outerApiClientMock
            .Setup(c => c.UpsertProviderAllowedCourse(
                Ukprn,
                LarsCode,
                It.Is<UpsertProviderAllowedCourseRequest>(r =>
                    r.UserId == "TestUser@education.gov.uk"
                    && r.UserDisplayName == "Test User"
                    && r.LastDateStarts == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<object>(new HttpResponseMessage(HttpStatusCode.OK), null, new RefitSettings(), null));

        var result = await sut.Index(LarsCode, CancellationToken.None) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.RestrictedCourseDetails);
        result.RouteValues!["larsCode"].Should().Be(LarsCode);

        sut.TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey]
            .Should().Be($"{LegalName.ToUpperInvariant()} is now allowed to deliver this training");

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddProviderToRestrictedCourse), Times.Once);

        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingConfirm_AndSessionMissing_ThenRedirectsWithoutCallingApi(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmAddProviderToRestrictedCourseController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<AddProviderToRestrictedCourseSessionModel>(SessionKeys.AddProviderToRestrictedCourse))
            .Returns((AddProviderToRestrictedCourseSessionModel?)null);

        var result = await sut.Index(LarsCode, CancellationToken.None) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.RestrictedCourseDetails);
        result.RouteValues!["larsCode"].Should().Be(LarsCode);

        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<UpsertProviderAllowedCourseRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddProviderToRestrictedCourse), Times.Never);
    }
}


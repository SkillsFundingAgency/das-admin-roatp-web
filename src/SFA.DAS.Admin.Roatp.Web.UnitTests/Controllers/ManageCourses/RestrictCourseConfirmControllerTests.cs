using System.Security.Claims;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
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
public class RestrictCourseConfirmControllerTests
{
    [Test, MoqAutoData]
    public void WhenGettingConfirm_AndSessionExists_ThenReturnsViewWithCourseDetails(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictCourseConfirmController sut,
        string larsCode)
    {
        var displayName = "Academic professional (Level 7)";
        sessionServiceMock
            .Setup(s => s.Get<AddRestrictedCourseSessionModel>(SessionKeys.AddRestrictedCourse))
            .Returns(new AddRestrictedCourseSessionModel
            {
                LarsCode = larsCode,
                DisplayName = displayName
            });

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.UnrestrictedCourseDetails, $"/unrestricted-courses/{larsCode}");

        var result = sut.Index(larsCode) as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be(RestrictCourseConfirmController.ViewPath);
        var model = result.Model as RestrictCourseConfirmViewModel;
        model.Should().NotBeNull();
        model!.LarsCode.Should().Be(larsCode);
        model.DisplayName.Should().Be(displayName);
        model.CancelUrl.Should().Be($"/unrestricted-courses/{larsCode}");
    }

    [Test, MoqAutoData]
    public void WhenGettingConfirm_AndSessionMissing_ThenRedirectsToUnrestrictedDetails(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictCourseConfirmController sut,
        string larsCode)
    {
        sessionServiceMock
            .Setup(s => s.Get<AddRestrictedCourseSessionModel>(SessionKeys.AddRestrictedCourse))
            .Returns((AddRestrictedCourseSessionModel?)null);

        var result = sut.Index(larsCode) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.UnrestrictedCourseDetails);
        result.RouteValues!["larsCode"].Should().Be(larsCode);
    }

    [Test, MoqAutoData]
    public void WhenGettingConfirm_AndSessionLarsCodeDoesNotMatch_ThenRedirectsToUnrestrictedDetails(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictCourseConfirmController sut,
        string larsCode)
    {
        sessionServiceMock
            .Setup(s => s.Get<AddRestrictedCourseSessionModel>(SessionKeys.AddRestrictedCourse))
            .Returns(new AddRestrictedCourseSessionModel
            {
                LarsCode = "different-code",
                DisplayName = "Other course"
            });

        var result = sut.Index(larsCode) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.UnrestrictedCourseDetails);
        result.RouteValues!["larsCode"].Should().Be(larsCode);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourse_ThenCallsApiDeletesSessionSetsBannerAndRedirects(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictCourseConfirmController sut,
        string larsCode)
    {
        sessionServiceMock
            .Setup(s => s.Get<AddRestrictedCourseSessionModel>(SessionKeys.AddRestrictedCourse))
            .Returns(new AddRestrictedCourseSessionModel
            {
                LarsCode = larsCode,
                DisplayName = "Academic professional (Level 7)"
            });

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Jane"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "Denver"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn", "jane@education.gov.uk")
                ], "test"))
            }
        };
        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

        var result = await sut.Index(larsCode, CancellationToken.None) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.RestrictedCourseDetails);
        result.RouteValues!["larsCode"].Should().Be(larsCode);
        sut.TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey]
            .Should().Be(RestrictCourseConfirmController.SuccessBannerMessage);

        outerApiClientMock.Verify(c => c.AddRestrictedCourse(
            It.Is<AddRestrictedCourseRequest>(r =>
                r.LarsCode == larsCode
                && r.UserId == "jane@education.gov.uk"
                && r.UserDisplayName == "Jane Denver"),
            It.IsAny<CancellationToken>()), Times.Once);
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddRestrictedCourse), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourse_AndSessionMissing_ThenRedirectsWithoutCallingApi(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictCourseConfirmController sut,
        string larsCode)
    {
        sessionServiceMock
            .Setup(s => s.Get<AddRestrictedCourseSessionModel>(SessionKeys.AddRestrictedCourse))
            .Returns((AddRestrictedCourseSessionModel?)null);

        var result = await sut.Index(larsCode, CancellationToken.None) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.UnrestrictedCourseDetails);
        outerApiClientMock.Verify(
            c => c.AddRestrictedCourse(It.IsAny<AddRestrictedCourseRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddRestrictedCourse), Times.Never);
    }
}

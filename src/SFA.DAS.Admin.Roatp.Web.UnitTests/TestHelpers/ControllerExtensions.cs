using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;

public static class ControllerExtensions
{
    public const string TestUrl = "http://testurl";

    public static Mock<IUrlHelper> AddUrlHelperMock(this Controller controller)
    {
        var urlHelperMock = new Mock<IUrlHelper>();
        controller.Url = urlHelperMock.Object;
        return urlHelperMock;
    }

    public static Mock<IUrlHelper> AddUrlForRoute(
        this Mock<IUrlHelper> urlHelperMock,
        string routeName,
        string url = TestUrl)
    {
        urlHelperMock
            .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName!.Equals(routeName))))
            .Returns(url);
        return urlHelperMock;
    }

    public static Controller AddTempData(this Controller sut)
    {
        if (sut.ControllerContext.HttpContext is null)
        {
            sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
        return sut;
    }

    public static Controller SetupAuthenticatedUser(this Controller sut)
    {
        var httpContext = sut.ControllerContext.HttpContext ?? new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "Test.User@example.com"),
            new Claim(ClaimTypes.Upn, "Test.User@example.com")
        ], "mock"));

        if (sut.ControllerContext.HttpContext is null)
        {
            sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        return sut;
    }
}

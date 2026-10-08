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

    public static Mock<IUrlHelper> AddUrlForRoute(this Mock<IUrlHelper> urlHelperMock, string routeName, string url = TestUrl)
    {
        urlHelperMock
            .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName!.Equals(routeName))))
            .Returns(url);
        return urlHelperMock;
    }

    public static Controller SetupAuthenticatedUser(this Controller sut)
    {
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = MockedUser.AuthenticatedUser
            }
        };
        return sut;
    }

    public static Controller AddTempData(this Controller sut)
    {
        sut.SetupHttpContext();
        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
        return sut;
    }

    public static Controller SetupHttpContext(this Controller sut)
    {
        if (sut.ControllerContext.HttpContext is null)
        {
            sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        return sut;
    }
}

using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Filters;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Filters;
public class RequiresSessionModelAttributeTests
{
    [Test, MoqAutoData]
    public void OnAuthorisation_WhenSessionModelExists_ShouldNotRedirect(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        AddProviderSessionModel sessionModel)
    {
        // Arrange
        string sessionKey = SessionKeys.AddProvider;
        string redirectrRoute = RouteNames.Home;

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(sessionKey)).Returns(sessionModel);

        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection()
                .AddSingleton(sessionServiceMock.Object)
                .BuildServiceProvider()
            },
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        var filterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

        RequiresSessionModelAttribute<AddProviderSessionModel> sut = new(sessionKey, redirectrRoute);

        // Act
        sut.OnAuthorization(filterContext);

        // Assert
        filterContext.Result.Should().BeNull();
    }

    [Test, MoqAutoData]
    public void OnAuthorisation_WhenSessionModelIsNull_ShouldRedirectToCorrectRoute(
        [Frozen] Mock<ISessionService> sessionServiceMock)
    {
        // Arrange
        string sessionKey = SessionKeys.AddProvider;
        string redirectrRoute = RouteNames.Home;

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(sessionKey)).Returns(() => null);

        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection()
                .AddSingleton(sessionServiceMock.Object)
                .BuildServiceProvider()
            },
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        var filterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

        RequiresSessionModelAttribute<AddProviderSessionModel> sut = new(sessionKey, redirectrRoute);

        // Act
        sut.OnAuthorization(filterContext);

        // Assert
        filterContext.Result.Should().BeOfType<RedirectToRouteResult>();
        var redirectResult = filterContext.Result! as RedirectToRouteResult;
        redirectResult!.RouteName.Should().Be(redirectrRoute);
    }
}

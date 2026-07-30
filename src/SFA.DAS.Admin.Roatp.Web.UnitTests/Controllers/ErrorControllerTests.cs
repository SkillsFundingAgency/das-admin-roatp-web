using System.Security.Claims;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers;

[TestFixture]
public class ErrorControllerTests
{
    [Test, MoqAutoData]
    public void WhenCalledErrorHandling_AndStatusIsForbidden_ThenReturnsAccessDeniedViewWithModel(
        [Frozen] Mock<IOptions<ApplicationConfiguration>> mockOptions,
        [Frozen] ApplicationConfiguration configuration,
        [Frozen] Mock<ILogger<ErrorController>> mockLogger,
        [Greedy] ErrorController sut)
    {
        mockOptions.Setup(c => c.Value).Returns(configuration);
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = MockedUser.Setup(Roles.RoatpAdminTeam)
            }
        };

        var result = sut.Index(StatusCodes.Status403Forbidden, string.Empty) as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be(ErrorController.AccessDeniedViewName);
        var model = result.Model as AccessDeniedViewModel;
        model!.HelpPageLink.Should().Be(configuration.DfESignInServiceHelpUrl);
    }

    [Test, MoqAutoData]
    public void WhenCalledErrorHandling_AndStatusIsForbidden_ThenLogsErrorWithUserAndRoles(
        [Frozen] Mock<IOptions<ApplicationConfiguration>> mockOptions,
        [Frozen] ApplicationConfiguration configuration,
        [Frozen] Mock<ILogger<ErrorController>> mockLogger,
        [Greedy] ErrorController sut)
    {
        mockOptions.Setup(c => c.Value).Returns(configuration);
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = MockedUser.Setup(Roles.RoatpAdminTeam)
            }
        };

        sut.Index(StatusCodes.Status403Forbidden, string.Empty);

        mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once());
    }

    [Test, MoqAutoData]
    public void WhenCalledErrorHandling_AndStatusIsForbiddenAndUserHasNoClaims_ThenLogsAndReturnsAccessDeniedView(
        [Frozen] Mock<IOptions<ApplicationConfiguration>> mockOptions,
        [Frozen] ApplicationConfiguration configuration,
        [Frozen] Mock<ILogger<ErrorController>> mockLogger,
        [Greedy] ErrorController sut)
    {
        mockOptions.Setup(c => c.Value).Returns(configuration);
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var result = sut.Index(StatusCodes.Status403Forbidden, string.Empty) as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be(ErrorController.AccessDeniedViewName);
        mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once());
    }

    [Test, MoqAutoData]
    public void WhenCalledErrorHandling_AndStatusIsNotFound_ThenReturnsPageNotFoundView(
        [Greedy] ErrorController sut)
    {
        var result = sut.Index(StatusCodes.Status404NotFound, string.Empty) as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be(ErrorController.PageNotFoundViewName);
    }

    [Test, MoqAutoData]
    public void WhenCalledErrorHandling_AndStatusIsAnythingElse_ThenReturnsServiceErrorView(
        [Greedy] ErrorController sut)
    {
        var result = sut.Index(StatusCodes.Status500InternalServerError, string.Empty) as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be(ErrorController.ServiceErrorViewName);
    }
}

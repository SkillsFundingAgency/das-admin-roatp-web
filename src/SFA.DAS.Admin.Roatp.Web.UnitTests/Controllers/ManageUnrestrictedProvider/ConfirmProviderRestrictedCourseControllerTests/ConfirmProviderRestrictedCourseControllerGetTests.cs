using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ConfirmProviderRestrictedCourseControllerTests;

public class ConfirmProviderRestrictedCourseControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string DisplayTitle = "Carpentry (Level 1)";
    private const string ProviderName = "Denton Business Services Limited";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";

    [Test, MoqAutoData]
    public void WhenGettingConfirm_AndSessionExists_ThenReturnsViewWithCourseDetails(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut)
    {
        SetupSession(sessionServiceMock);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);

        var result = sut.Index(Ukprn) as ViewResult;
        var model = result?.Model as ConfirmProviderRestrictedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ConfirmProviderRestrictedCourseController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.ProviderName.Should().Be(ProviderName);
            model.CourseDisplayTitle.Should().Be(DisplayTitle);
            model.LarsCode.Should().Be(LarsCode);
            model.CancelUrl.Should().Be(RestrictedCoursesUrl);
        }
    }

    [Test, MoqAutoData]
    public void WhenGettingConfirm_AndSessionIsMissing_ThenRedirectsToRestrictedApprenticeshipsList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<ProviderRestrictedCourseSessionModel>(SessionKeys.ProviderRestrictedCourse))
            .Returns((ProviderRestrictedCourseSessionModel?)null);

        var result = sut.Index(Ukprn) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test, MoqAutoData]
    public void WhenGettingConfirm_AndSessionUkprnDoesNotMatch_ThenRedirectsToRestrictedApprenticeshipsList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut)
    {
        SetupSession(sessionServiceMock, ukprn: 99999999);

        var result = sut.Index(Ukprn) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    private static void SetupSession(Mock<ISessionService> sessionServiceMock, int ukprn = Ukprn)
    {
        sessionServiceMock
            .Setup(s => s.Get<ProviderRestrictedCourseSessionModel>(SessionKeys.ProviderRestrictedCourse))
            .Returns(new ProviderRestrictedCourseSessionModel
            {
                Ukprn = ukprn,
                LarsCode = LarsCode,
                CourseDisplayTitle = DisplayTitle,
                ProviderName = ProviderName
            });
    }
}

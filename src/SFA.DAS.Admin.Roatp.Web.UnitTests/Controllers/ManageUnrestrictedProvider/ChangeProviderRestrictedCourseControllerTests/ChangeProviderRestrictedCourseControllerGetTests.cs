using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ChangeProviderRestrictedCourseControllerTests;

[TestFixture]
public class ChangeProviderRestrictedCourseControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";
    private static readonly DateTime LastDateStarts = new(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public void WhenGettingChange_AndCourseHasLastStartDate_ThenReturnsViewWithDate(
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Greedy] ChangeProviderRestrictedCourseController sut)
    {
        SetupCachedCourse(applicationCacheMock, LastDateStarts);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);

        var result = sut.Index(Ukprn, LarsCode) as ViewResult;
        var model = result?.Model as ChangeProviderRestrictedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeProviderRestrictedCourseController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.LarsCode.Should().Be(LarsCode);
            model.DisplayTitle.Should().Be("Electrical (Level 3)");
            model.HasLastDateStarts.Should().BeTrue();
            model.LastDateStarts.Should().Be(LastDateStarts);
            model.LastDateStartsText.Should().Be("12 Jul 2026");
            model.CancelUrl.Should().Be(RestrictedCoursesUrl);
            model.SelectedOption.Should().BeNull();
        }
    }

    [Test, MoqAutoData]
    public void WhenGettingChange_AndCourseHasNoLastStartDate_ThenReturnsViewWithoutDate(
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Greedy] ChangeProviderRestrictedCourseController sut)
    {
        SetupCachedCourse(applicationCacheMock, lastDateStarts: null);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);

        var result = sut.Index(Ukprn, LarsCode) as ViewResult;
        var model = result?.Model as ChangeProviderRestrictedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeProviderRestrictedCourseController.ViewPath);
            model.Should().NotBeNull();
            model!.DisplayTitle.Should().Be("Electrical (Level 3)");
            model.HasLastDateStarts.Should().BeFalse();
            model.LastDateStarts.Should().BeNull();
            model.CancelUrl.Should().Be(RestrictedCoursesUrl);
        }
    }

    [Test, MoqAutoData]
    public void WhenGettingChange_AndCacheIsMissing_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Greedy] ChangeProviderRestrictedCourseController sut)
    {
        SetupCacheMiss(applicationCacheMock);

        var result = sut.Index(Ukprn, LarsCode) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test, MoqAutoData]
    public void WhenGettingChange_AndCourseIsNotInCache_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Greedy] ChangeProviderRestrictedCourseController sut)
    {
        SetupCachedCourse(applicationCacheMock, LastDateStarts, larsCode: "999");

        var result = sut.Index(Ukprn, LarsCode) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test]
    public void ChangeProviderRestrictedCourseOptions_ExposeExpectedValues()
    {
        using (new AssertionScope())
        {
            ChangeProviderRestrictedCourseOptions.AddOrChange.Should().Be("AddOrChange");
            ChangeProviderRestrictedCourseOptions.Remove.Should().Be("Remove");
        }
    }

    private static void SetupCachedCourse(
        Mock<IApplicationCacheService> applicationCacheMock,
        DateTime? lastDateStarts,
        string larsCode = LarsCode)
    {
        List<RestrictedApprenticeshipModel>? courses =
        [
            new RestrictedApprenticeshipModel
            {
                LarsCode = larsCode,
                Title = "Electrical",
                Level = 3,
                LastDateStarts = lastDateStarts,
                IsClosedToNewStarts = false
            }
        ];

        applicationCacheMock
            .Setup(c => c.TryGet(ApplicationCacheKeys.RestrictedApprenticeships(Ukprn), out courses))
            .Returns(true);
    }

    private static void SetupCacheMiss(Mock<IApplicationCacheService> applicationCacheMock)
    {
        List<RestrictedApprenticeshipModel>? courses = null;
        applicationCacheMock
            .Setup(c => c.TryGet(ApplicationCacheKeys.RestrictedApprenticeships(Ukprn), out courses))
            .Returns(false);
    }
}

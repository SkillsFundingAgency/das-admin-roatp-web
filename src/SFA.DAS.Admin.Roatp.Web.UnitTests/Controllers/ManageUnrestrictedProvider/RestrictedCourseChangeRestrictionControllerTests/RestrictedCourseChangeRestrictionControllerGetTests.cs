using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.RestrictedCourseChangeRestrictionControllerTests;

[TestFixture]
public class RestrictedCourseChangeRestrictionControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string ProviderName = "Denton Business Services Limited";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";
    private static readonly DateTime LastDateStarts = new(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenGettingChange_AndCourseHasLastStartDate_ThenReturnsViewWithDateAndStoresSession(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupRestrictedApprenticeship(outerApiClientMock, LastDateStarts);
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictedCourseChangeRestrictionViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RestrictedCourseChangeRestrictionController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.LarsCode.Should().Be(LarsCode);
            model.CourseDisplayTitle.Should().Be("Electrical (Level 3)");
            model.HasLastDateStarts.Should().BeTrue();
            model.LastDateStarts.Should().Be(LastDateStarts);
            model.LastDateStartsText.Should().Be("12 Jul 2026");
            model.CancelUrl.Should().Be(RestrictedCoursesUrl);
            model.SelectedOption.Should().BeNull();
        }

        sessionServiceMock.Verify(
            s => s.Set(
                SessionKeys.ProviderRestrictedCourseChangeRestriction,
                It.Is<ChangeRestrictedCourseRestrictionSessionModel>(m =>
                    m.Ukprn == Ukprn
                    && m.LarsCode == LarsCode
                    && m.CourseDisplayTitle == "Electrical (Level 3)"
                    && m.ProviderName == ProviderName
                    && m.LastDateStarts == LastDateStarts)),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChange_AndCourseHasNoLastStartDate_ThenReturnsViewWithoutDateAndStoresSession(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupRestrictedApprenticeship(outerApiClientMock, lastDateStarts: null);
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictedCourseChangeRestrictionViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RestrictedCourseChangeRestrictionController.ViewPath);
            model.Should().NotBeNull();
            model!.CourseDisplayTitle.Should().Be("Electrical (Level 3)");
            model.HasLastDateStarts.Should().BeFalse();
            model.LastDateStarts.Should().BeNull();
            model.CancelUrl.Should().Be(RestrictedCoursesUrl);
        }

        sessionServiceMock.Verify(
            s => s.Set(
                SessionKeys.ProviderRestrictedCourseChangeRestriction,
                It.Is<ChangeRestrictedCourseRestrictionSessionModel>(m =>
                    m.LastDateStarts == null
                    && m.ProviderName == ProviderName)),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChange_AndRestrictedApprenticeshipsAreNotFound_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupRestrictedApprenticeship(outerApiClientMock, statusCode: HttpStatusCode.NotFound);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderRestrictedCourseChangeRestriction, It.IsAny<ChangeRestrictedCourseRestrictionSessionModel>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChange_AndCourseIsNotRestrictedForProvider_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupRestrictedApprenticeship(outerApiClientMock, LastDateStarts, larsCode: "999");

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderRestrictedCourseChangeRestriction, It.IsAny<ChangeRestrictedCourseRestrictionSessionModel>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChange_AndProviderNameIsMissing_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupRestrictedApprenticeship(outerApiClientMock, LastDateStarts);
        sessionServiceMock.SetupProviderNameMissing();
        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound),
                null,
                new RefitSettings(),
                null));

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderRestrictedCourseChangeRestriction, It.IsAny<ChangeRestrictedCourseRestrictionSessionModel>()),
            Times.Never);
    }

    [Test]
    public void RestrictedCourseChangeRestrictionOptions_ExposeExpectedValues()
    {
        using (new AssertionScope())
        {
            RestrictedCourseChangeRestrictionOptions.AddOrChange.Should().Be("AddOrChange");
            RestrictedCourseChangeRestrictionOptions.Remove.Should().Be("Remove");
        }
    }

    private static void SetupRestrictedApprenticeship(
        Mock<IOuterApiClient> outerApiClientMock,
        DateTime? lastDateStarts = null,
        string larsCode = LarsCode,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var courses = statusCode == HttpStatusCode.OK
            ? new List<ProviderRestrictedApprenticeshipModel>
            {
                new()
                {
                    LarsCode = larsCode,
                    Title = "Electrical",
                    Level = 3,
                    LastDateStarts = lastDateStarts,
                    IsClosedToNewStarts = false
                }
            }
            : [];

        var httpResponse = new HttpResponseMessage(statusCode);
        outerApiClientMock
            .Setup(c => c.GetRestrictedApprenticeships(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedApprenticeshipsResponse>(
                httpResponse,
                new GetRestrictedApprenticeshipsResponse { Courses = courses },
                new RefitSettings(),
                null));
    }
}

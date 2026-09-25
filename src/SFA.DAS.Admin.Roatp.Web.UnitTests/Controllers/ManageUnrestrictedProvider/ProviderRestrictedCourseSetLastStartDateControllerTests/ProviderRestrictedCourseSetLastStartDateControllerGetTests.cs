using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ProviderRestrictedCourseSetLastStartDateControllerTests;

[TestFixture]
public class ProviderRestrictedCourseSetLastStartDateControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string DisplayTitle = "Electrical (Level 3)";
    private const string ProviderName = "Denton Business Services Limited";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";
    private static readonly DateTime CourseLastDateStarts = new(2028, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndSessionExists_ThenReturnsView(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderRestrictedCourseSetLastStartDateController sut)
    {
        SetupSession(sessionServiceMock);
        SetupCourseLastDateStarts(outerApiClientMock);
        sut.AddTempData();
        sut.TempData[TempDataKeys.ProviderLegalName] = ProviderName;
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(Ukprn) as ViewResult;
        var model = result?.Model as ProviderRestrictedCourseSetLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ProviderRestrictedCourseSetLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.ProviderName.Should().Be(ProviderName);
            model.DisplayTitle.Should().Be(DisplayTitle);
            model.LarsCode.Should().Be(LarsCode);
            model.CourseLastDateStarts.Should().Be(CourseLastDateStarts);
            model.CancelUrl.Should().Be(RestrictedCoursesUrl);
            model.Day.Should().BeNull();
            model.Month.Should().BeNull();
            model.Year.Should().BeNull();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndSessionIsMissing_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ProviderRestrictedCourseSetLastStartDateController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<ProviderRestrictedCourseSessionModel>(SessionKeys.ProviderRestrictedCourse))
            .Returns((ProviderRestrictedCourseSessionModel?)null);

        var result = await sut.Index(Ukprn) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndSessionUkprnDoesNotMatch_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ProviderRestrictedCourseSetLastStartDateController sut)
    {
        SetupSession(sessionServiceMock, ukprn: 99999999);

        var result = await sut.Index(Ukprn) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndOrganisationIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderRestrictedCourseSetLastStartDateController sut,
        GetOrganisationResponse organisationResponse)
    {
        SetupSession(sessionServiceMock);
        sut.AddTempData();
        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound),
                organisationResponse,
                new RefitSettings(),
                null));

        var result = await sut.Index(Ukprn);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupSession(Mock<ISessionService> sessionServiceMock, int ukprn = Ukprn)
    {
        sessionServiceMock
            .Setup(s => s.Get<ProviderRestrictedCourseSessionModel>(SessionKeys.ProviderRestrictedCourse))
            .Returns(new ProviderRestrictedCourseSessionModel
            {
                Ukprn = ukprn,
                LarsCode = LarsCode,
                Title = "Electrical",
                Level = 3,
                CourseDisplayTitle = DisplayTitle
            });
    }

    private static void SetupCourseLastDateStarts(Mock<IOuterApiClient> outerApiClientMock)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                new GetRestrictedCourseDetailsResponse
                {
                    LarsCode = LarsCode,
                    IfateReferenceNumber = "ST0001",
                    CourseName = "Electrical",
                    Route = "Construction",
                    LastDateStarts = CourseLastDateStarts
                },
                new RefitSettings(),
                null));
    }
}

using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageRestrictedProvider.ProviderAllowedCoursesControllerTests;

[TestFixture]
public class ProviderAllowedCoursesControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string ProviderName = "Denton Business Services Limited";
    private const string ProviderSummaryUrl = "/providers/10019900";
    private const string AllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndProviderIsRestricted_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        var response = CreateCoursesResponse();
        SetupRestrictedProvider(sessionServiceMock, ukprnServiceMock, outerApiClientMock, response);
        SetupUrlHelper(sut);

        var result = await sut.Index(Ukprn, CourseType.Apprenticeship, CancellationToken.None) as ViewResult;
        var model = result?.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ProviderAllowedCoursesController.ViewPath);
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be(ProviderName);
            model.BackLinkUrl.Should().Be(ProviderSummaryUrl);
            model.BackLinkText.Should().Be(AllowedCoursesViewModel.BackLinkTextValue);
            model.AddUrl.Should().Be(AllowedCoursesUrl);
            model.CourseType.Should().Be(CourseType.Apprenticeship);
            model.PageContent.PageHeading.Should().Be("Manage apprenticeships this provider is allowed to deliver");
            model.HasCourses.Should().BeTrue();
            model.HasNoCourses.Should().BeFalse();
            model.Courses.Select(course => course.LarsCode).Should().Equal("105", "200");
            model.Courses.Select(course => course.DeliveryStatus).Should().Equal(
                DeliveryStatus.LastStartDateAdded,
                DeliveryStatus.ClosedToNewStarts);
            model.Courses.Should().OnlyContain(course => course.ChangeUrl == AllowedCoursesUrl);
            model.TotalCountDescription.Should().Be("2 courses");
        }

        outerApiClientMock.Verify(
            c => c.GetAllowedCourses(Ukprn, CourseType.Apprenticeship, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndThereAreNoCourses_ThenReturnsEmptyState(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(
            sessionServiceMock,
            ukprnServiceMock,
            outerApiClientMock,
            new GetAllowedCoursesResponse { AllowedCourses = [] });
        SetupUrlHelper(sut);

        var result = await sut.Index(Ukprn, CourseType.Apprenticeship, CancellationToken.None) as ViewResult;
        var model = result?.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model.Should().NotBeNull();
            model!.HasNoCourses.Should().BeTrue();
            model.HasCourses.Should().BeFalse();
            model.PageContent.EmptyListText.Should().Be("There are currently no apprenticeships added to this list.");
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndCourseTypeIsShortCourse_ThenUsesUnitContent(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(
            sessionServiceMock,
            ukprnServiceMock,
            outerApiClientMock,
            new GetAllowedCoursesResponse { AllowedCourses = [] },
            CourseType.ShortCourse);
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            CourseType.ShortCourse,
            CancellationToken.None) as ViewResult;
        var model = result?.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model.Should().NotBeNull();
            model!.CourseType.Should().Be(CourseType.ShortCourse);
            model.PageContent.AddButtonText.Should().Be("Add an apprenticeship unit");
            model.PageContent.ListHeading.Should().Be("Apprenticeship units this provider can deliver");
        }

        outerApiClientMock.Verify(
            c => c.GetAllowedCourses(Ukprn, CourseType.ShortCourse, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndCourseTypeIsInvalid_ThenReturnsNotFound(
        [Greedy] ProviderAllowedCoursesController sut)
    {
        var result = await sut.Index(Ukprn, (CourseType)0, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndProviderIsUnrestricted_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
        ukprnServiceMock.SetupOrganisationRestriction(Ukprn, isRestricted: false);

        var result = await sut.Index(Ukprn, CourseType.Apprenticeship, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        outerApiClientMock.Verify(
            c => c.GetAllowedCourses(It.IsAny<int>(), It.IsAny<CourseType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndOrganisationIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);

        var result = await sut.Index(Ukprn, CourseType.Apprenticeship, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndProviderNameIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        sessionServiceMock.SetupProviderNameMissing();
        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<GetOrganisationResponse>(HttpStatusCode.NotFound, includeException: false));

        var result = await sut.Index(Ukprn, CourseType.Apprenticeship, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().BeOfType<NotFoundResult>();
            ukprnServiceMock.Verify(
                s => s.GetOrganisationAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndAllowedCoursesAreNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(sessionServiceMock, ukprnServiceMock, outerApiClientMock, statusCode: HttpStatusCode.NotFound);

        var result = await sut.Index(Ukprn, CourseType.Apprenticeship, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(
            sessionServiceMock,
            ukprnServiceMock,
            outerApiClientMock,
            statusCode: HttpStatusCode.InternalServerError);

        var act = () => sut.Index(Ukprn, CourseType.Apprenticeship, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndCoursesAreNull_ThenReturnsViewWithNoCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(
            sessionServiceMock,
            ukprnServiceMock,
            outerApiClientMock,
            new GetAllowedCoursesResponse { AllowedCourses = null! });
        SetupUrlHelper(sut);

        var result = await sut.Index(Ukprn, CourseType.Apprenticeship, CancellationToken.None) as ViewResult;
        var model = result?.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model.Should().NotBeNull();
            model!.HasNoCourses.Should().BeTrue();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndResponseContentIsNull_ThenReturnsViewWithNoCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(sessionServiceMock, ukprnServiceMock, outerApiClientMock, response: null);
        SetupUrlHelper(sut);

        var result = await sut.Index(Ukprn, CourseType.Apprenticeship, CancellationToken.None) as ViewResult;
        var model = result?.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model.Should().NotBeNull();
            model!.HasNoCourses.Should().BeTrue();
        }
    }

    private static void SetupUrlHelper(ProviderAllowedCoursesController sut)
    {
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderSummary, ProviderSummaryUrl)
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, AllowedCoursesUrl);
    }

    private static void SetupRestrictedProvider(
        Mock<ISessionService> sessionServiceMock,
        Mock<IUkprnService> ukprnServiceMock,
        Mock<IOuterApiClient> outerApiClientMock,
        GetAllowedCoursesResponse? response = null,
        CourseType courseType = CourseType.Apprenticeship,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
        ukprnServiceMock.SetupOrganisationRestriction(Ukprn, isRestricted: true, courseType);
        outerApiClientMock
            .Setup(c => c.GetAllowedCourses(Ukprn, courseType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create(statusCode, response));
    }

    private static GetAllowedCoursesResponse CreateCoursesResponse()
        => new()
        {
            AllowedCourses =
            [
                new ProviderAllowedCourseModel
                {
                    LarsCode = "200",
                    Title = "Zebra course",
                    Level = 3,
                    LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
                    IsClosedToNewStarts = true
                },
                new ProviderAllowedCourseModel
                {
                    LarsCode = "105",
                    Title = "Alpha course",
                    Level = 6,
                    LastDateStarts = DateTime.UtcNow.Date.AddDays(5),
                    IsClosedToNewStarts = false
                }
            ]
        };
}

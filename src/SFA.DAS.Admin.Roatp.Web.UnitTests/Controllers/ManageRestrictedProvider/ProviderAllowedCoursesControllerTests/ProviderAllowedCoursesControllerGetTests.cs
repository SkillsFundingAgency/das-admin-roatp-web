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
using SFA.DAS.Admin.Roatp.Web.Extensions;
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
    private const string AddAllowedCourseUrl = "/providers/10019900/Apprenticeship/allowed-courses/add";

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndProviderIsRestricted_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        var response = CreateCoursesResponse();
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, response);
        SetupUrlHelper(sut);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken()) as ViewResult;
        var model = result?.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ProviderAllowedCoursesController.ViewPath);
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be(ProviderName);
            model.BackLinkUrl.Should().Be(ProviderSummaryUrl);
            model.BackLinkText.Should().Be(AllowedCoursesViewModel.BackLinkTextValue);
            model.AddUrl.Should().Be(AddAllowedCourseUrl);
            model.RouteLearningType.Should().Be(RouteLearningType.Apprenticeship);
            model.IsApprenticeship.Should().BeTrue();
            model.IsApprenticeshipUnit.Should().BeFalse();
            model.HasCourses.Should().BeTrue();
            model.HasNoCourses.Should().BeFalse();
            model.Courses.Select(course => course.LarsCode).Should().Equal("105", "200");
            model.Courses.Select(course => course.DeliveryStatus).Should().Equal(
                DeliveryStatus.LastStartDateAdded,
                DeliveryStatus.ClosedToNewStarts);
            model.Courses.Should().OnlyContain(course => course.ChangeUrl == AllowedCoursesUrl);
            model.TotalCountDescription.Should().Be("2 courses");
            model.HasSuccessBanner.Should().BeFalse();
        }

        outerApiClientMock.Verify(
            c => c.GetAllowedCourses(Ukprn, CourseType.Apprenticeship, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndThereAreNoCourses_ThenReturnsEmptyState(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            new GetAllowedCoursesResponse { AllowedCourses = [] });
        SetupUrlHelper(sut);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken()) as ViewResult;
        var model = result?.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model.Should().NotBeNull();
            model!.HasNoCourses.Should().BeTrue();
            model.HasCourses.Should().BeFalse();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndRouteLearningTypeIsApprenticeshipUnit_ThenCallsOuterApiWithShortCourse(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            new GetAllowedCoursesResponse { AllowedCourses = [] },
            RouteLearningType.ApprenticeshipUnit);
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            new CancellationToken()) as ViewResult;
        var model = result?.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model.Should().NotBeNull();
            model!.RouteLearningType.Should().Be(RouteLearningType.ApprenticeshipUnit);
            model.IsApprenticeshipUnit.Should().BeTrue();
            model.IsApprenticeship.Should().BeFalse();
        }

        outerApiClientMock.Verify(
            c => c.GetAllowedCourses(Ukprn, CourseType.ShortCourse, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndTempDataContainsSuccessBanner_ThenModelHasSuccessBanner(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        const string successMessage = "Electrical (Level 3) added to approved list";
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            new GetAllowedCoursesResponse { AllowedCourses = [] });
        SetupUrlHelper(sut);
        sut.AddTempData();
        sut.TempData[ProviderAllowedCoursesController.SuccessBannerTempDataKey] = successMessage;

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken()) as ViewResult;
        var model = result?.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model.Should().NotBeNull();
            model!.SuccessBannerMessage.Should().Be(successMessage);
            model.HasSuccessBanner.Should().BeTrue();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndRouteLearningTypeIsInvalid_ThenReturnsNotFound(
        [Greedy] ProviderAllowedCoursesController sut)
    {
        var result = await sut.Index(Ukprn, (RouteLearningType)0, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndRouteLearningTypeIsApprenticeshipAndProviderIsUnrestricted_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        ukprnServiceMock.SetupOrganisationRestriction(Ukprn, isRestricted: false);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken()) as RedirectToRouteResult;

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
    public async Task WhenGettingAllowedCourses_AndRouteLearningTypeIsApprenticeshipUnitAndProviderIsNotRestricted_ThenReturnsView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            new GetAllowedCoursesResponse { AllowedCourses = [] },
            RouteLearningType.ApprenticeshipUnit,
            isRestricted: false);
        SetupUrlHelper(sut);

        var result = await sut.Index(Ukprn, RouteLearningType.ApprenticeshipUnit, new CancellationToken()) as ViewResult;
        var model = result?.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ProviderAllowedCoursesController.ViewPath);
            model.Should().NotBeNull();
            model!.RouteLearningType.Should().Be(RouteLearningType.ApprenticeshipUnit);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndOrganisationIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndAllowedCoursesAreNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, statusCode: HttpStatusCode.NotFound);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            statusCode: HttpStatusCode.InternalServerError);

        var act = () => sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken());

        await act.Should().ThrowAsync<ApiException>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndCoursesAreNull_ThenReturnsViewWithNoCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            new GetAllowedCoursesResponse { AllowedCourses = null! });
        SetupUrlHelper(sut);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken()) as ViewResult;
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
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, response: null);
        SetupUrlHelper(sut);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken()) as ViewResult;
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
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, AllowedCoursesUrl)
            .AddUrlForRoute(RouteNames.AddAllowedCourse, AddAllowedCourseUrl);
    }

    private static void SetupRestrictedProvider(
        Mock<IUkprnService> ukprnServiceMock,
        Mock<IOuterApiClient> outerApiClientMock,
        GetAllowedCoursesResponse? response = null,
        RouteLearningType routeLearningType = RouteLearningType.Apprenticeship,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        bool isRestricted = true)
    {
        var courseType = routeLearningType.ToCourseType();
        ukprnServiceMock.SetupOrganisationRestriction(Ukprn, isRestricted, courseType, ProviderName);
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

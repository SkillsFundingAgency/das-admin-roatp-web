using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageRestrictedProvider.AddAllowedCourseLastStartDateControllerTests;

[TestFixture]
public class AddAllowedCourseLastStartDateControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string CourseTitle = "Carpentry";
    private const int CourseLevel = 1;
    private const string ProviderName = "Denton Business Services Limited";
    private const string AllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";
    private static readonly DateTime CourseLastDateStarts = new(2028, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndProviderIsRestricted_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new CancellationToken()) as ViewResult;
        var model = result?.Model as AddAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.RouteLearningType.Should().Be(RouteLearningType.Apprenticeship);
            model.IsApprenticeship.Should().BeTrue();
            model.IsApprenticeshipUnit.Should().BeFalse();
            model.CourseDisplayTitle.Should().Be("Carpentry (Level 1)");
            model.LarsCode.Should().Be(LarsCode);
            model.CourseLastDateStarts.Should().Be(CourseLastDateStarts);
            model.Day.Should().BeNull();
            model.Month.Should().BeNull();
            model.Year.Should().BeNull();
            model.CancelUrl.Should().Be(AllowedCoursesUrl);
            model.Should().NotBeAssignableTo<IBackLink>();
        }

        outerApiClientMock.Verify(
            c => c.GetProviderAllowedCourse(Ukprn, LarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        coursesServiceMock.Verify(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndAllowedCourseHasLastDateStarts_ThenRedirectsToAllowedCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        var lastDateStarts = new DateTime(2026, 8, 2, 0, 0, 0, DateTimeKind.Unspecified);
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock, allowedCourse: CreateAllowedCourse(lastDateStarts));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new CancellationToken()) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderAllowedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["routeLearningType"].Should().Be(RouteLearningType.Apprenticeship);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndRouteLearningTypeIsApprenticeshipUnit_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        const string allowedUnitsUrl = "/providers/10019900/ApprenticeshipUnit/allowed-courses";
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            coursesServiceMock,
            RouteLearningType.ApprenticeshipUnit);
        var urlHelperMock = sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, allowedUnitsUrl);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            LarsCode,
            new CancellationToken()) as ViewResult;
        var model = result?.Model as AddAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.RouteLearningType.Should().Be(RouteLearningType.ApprenticeshipUnit);
            model.IsApprenticeshipUnit.Should().BeTrue();
            model.IsApprenticeship.Should().BeFalse();
            model.CancelUrl.Should().Be(allowedUnitsUrl);
        }

        outerApiClientMock.Verify(
            c => c.GetProviderAllowedCourse(Ukprn, LarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        urlHelperMock.Verify(
            m => m.RouteUrl(It.Is<UrlRouteContext>(c =>
                c.RouteName == RouteNames.ProviderAllowedCourses
                && HasRouteValue(c, "ukprn", Ukprn)
                && HasRouteValue(c, "routeLearningType", RouteLearningType.ApprenticeshipUnit))),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndRouteLearningTypeIsInvalid_ThenReturnsNotFound(
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        var result = await sut.Index(Ukprn, (RouteLearningType)0, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndRouteLearningTypeIsApprenticeshipAndProviderIsUnrestricted_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        ukprnServiceMock.SetupOrganisationRestriction(Ukprn, isRestricted: false);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
        outerApiClientMock.Verify(
            c => c.GetProviderAllowedCourse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndRouteLearningTypeIsApprenticeshipUnitAndProviderIsNotRestricted_ThenReturnsView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            coursesServiceMock,
            RouteLearningType.ApprenticeshipUnit);
        ukprnServiceMock.SetupOrganisationRestriction(
            Ukprn,
            isRestricted: false,
            CourseType.ShortCourse,
            ProviderName);
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            LarsCode,
            new CancellationToken()) as ViewResult;
        var model = result?.Model as AddAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.RouteLearningType.Should().Be(RouteLearningType.ApprenticeshipUnit);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndOrganisationIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndAllowedCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock, HttpStatusCode.NotFound);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
        coursesServiceMock.Verify(c => c.GetCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndAllowedCourseContentIsNull_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock, HttpStatusCode.InternalServerError);

        var act = () => sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        await act.Should().ThrowAsync<ApiException>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCourseResponse?)null);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastStartDate_AndCourseIsNotOfLearningType_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCourse(CourseType.ShortCourse));

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupUrlHelper(AddAllowedCourseLastStartDateController sut)
    {
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, AllowedCoursesUrl);
    }

    private static void SetupRestrictedProvider(
        Mock<IUkprnService> ukprnServiceMock,
        Mock<IOuterApiClient> outerApiClientMock,
        Mock<ICoursesService> coursesServiceMock,
        RouteLearningType routeLearningType = RouteLearningType.Apprenticeship)
    {
        var courseType = routeLearningType.ToCourseType();
        ukprnServiceMock.SetupOrganisationRestriction(Ukprn, isRestricted: true, courseType, ProviderName);
        SetupAllowedCourse(outerApiClientMock, allowedCourse: CreateAllowedCourse());
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCourse(courseType));
    }

    private static void SetupAllowedCourse(
        Mock<IOuterApiClient> outerApiClientMock,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        ProviderAllowedCourseModel? allowedCourse = null)
    {
        outerApiClientMock
            .Setup(c => c.GetProviderAllowedCourse(Ukprn, LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create(statusCode, allowedCourse));
    }

    private static ProviderAllowedCourseModel CreateAllowedCourse(DateTime? lastDateStarts = null)
        => new()
        {
            LarsCode = LarsCode,
            Title = CourseTitle,
            Level = CourseLevel,
            LastDateStarts = lastDateStarts,
            IsClosedToNewStarts = false
        };

    private static GetCourseResponse CreateCourse(CourseType courseType)
        => new()
        {
            LarsCode = LarsCode,
            Title = CourseTitle,
            Level = CourseLevel,
            CourseType = courseType,
            LastDateStarts = CourseLastDateStarts
        };

    private static bool HasRouteValue(UrlRouteContext context, string propertyName, object expected)
        => Equals(context.Values?.GetType().GetProperty(propertyName)?.GetValue(context.Values), expected);
}

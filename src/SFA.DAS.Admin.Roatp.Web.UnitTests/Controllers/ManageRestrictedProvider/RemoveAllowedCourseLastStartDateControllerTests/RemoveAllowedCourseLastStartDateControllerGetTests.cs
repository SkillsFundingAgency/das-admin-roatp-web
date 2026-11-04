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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageRestrictedProvider.RemoveAllowedCourseLastStartDateControllerTests;

[TestFixture]
public class RemoveAllowedCourseLastStartDateControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string CourseTitle = "Carpentry";
    private const int CourseLevel = 1;
    private const string UnitCourseTitle = "Permanent modular building assembly - Apprenticeship unit";
    private const int UnitCourseLevel = 4;
    private const string ProviderName = "Denton Business Services Limited";
    private const string AllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";
    private static readonly DateTime LastDateStarts = new(2026, 8, 2, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndProviderIsRestricted_ThenReturnsViewWithCourseDetails(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        sut.SetupAuthenticatedUser();
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new CancellationToken()) as ViewResult;
        var model = result?.Model as RemoveAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RemoveAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.RouteLearningType.Should().Be(RouteLearningType.Apprenticeship);
            model.CourseDisplayTitle.Should().Be("Carpentry (Level 1)");
            model.LarsCode.Should().Be(LarsCode);
            model.CancelUrl.Should().Be(AllowedCoursesUrl);
            model.Should().NotBeAssignableTo<IBackLink>();
        }

        outerApiClientMock.Verify(
            c => c.GetProviderAllowedCourse(Ukprn, LarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        coursesServiceMock.Verify(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndAllowedCourseHasNoLastDateStarts_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock, allowedCourse: CreateAllowedCourse());
        sut.SetupAuthenticatedUser();

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
        coursesServiceMock.Verify(c => c.GetCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndRouteLearningTypeIsApprenticeshipUnit_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        const string allowedUnitsUrl = "/providers/10019900/ApprenticeshipUnit/allowed-courses";
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            coursesServiceMock,
            RouteLearningType.ApprenticeshipUnit);
        sut.SetupAuthenticatedUser();
        var urlHelperMock = sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, allowedUnitsUrl);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            LarsCode,
            new CancellationToken()) as ViewResult;
        var model = result?.Model as RemoveAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RemoveAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.RouteLearningType.Should().Be(RouteLearningType.ApprenticeshipUnit);
            model.CourseDisplayTitle.Should().Be("Permanent modular building assembly - Apprenticeship unit (Level 4)");
            model.LarsCode.Should().Be(LarsCode);
            model.CancelUrl.Should().Be(allowedUnitsUrl);
            model.Should().NotBeAssignableTo<IBackLink>();
        }

        urlHelperMock.Verify(
            m => m.RouteUrl(It.Is<UrlRouteContext>(c =>
                c.RouteName == RouteNames.ProviderAllowedCourses
                && HasRouteValue(c, "ukprn", Ukprn)
                && HasRouteValue(c, "routeLearningType", RouteLearningType.ApprenticeshipUnit))),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndRouteLearningTypeIsApprenticeshipUnitAndAllowedCourseHasNoLastDateStarts_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            coursesServiceMock,
            RouteLearningType.ApprenticeshipUnit);
        SetupAllowedCourse(
            outerApiClientMock,
            allowedCourse: CreateAllowedCourse(title: UnitCourseTitle, level: UnitCourseLevel));
        sut.SetupAuthenticatedUser();

        var result = await sut.Index(Ukprn, RouteLearningType.ApprenticeshipUnit, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
        coursesServiceMock.Verify(c => c.GetCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndRouteLearningTypeIsInvalid_ThenReturnsNotFound(
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        var result = await sut.Index(Ukprn, (RouteLearningType)0, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndRouteLearningTypeIsApprenticeshipAndProviderIsUnrestricted_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        ukprnServiceMock.SetupOrganisationRestriction(Ukprn, isRestricted: false);
        sut.SetupAuthenticatedUser();

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
    public async Task WhenGettingRemoveLastStartDate_AndRouteLearningTypeIsApprenticeshipUnitAndProviderIsNotRestricted_ThenReturnsView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
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
        sut.SetupAuthenticatedUser();
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            LarsCode,
            new CancellationToken()) as ViewResult;
        var model = result?.Model as RemoveAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RemoveAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.RouteLearningType.Should().Be(RouteLearningType.ApprenticeshipUnit);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndOrganisationIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);
        sut.SetupAuthenticatedUser();

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndAllowedCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock, HttpStatusCode.NotFound);
        sut.SetupAuthenticatedUser();

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
        coursesServiceMock.Verify(c => c.GetCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndAllowedCourseContentIsNull_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock);
        sut.SetupAuthenticatedUser();

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock, HttpStatusCode.InternalServerError);
        sut.SetupAuthenticatedUser();

        var act = () => sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        await act.Should().ThrowAsync<ApiException>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCourseResponse?)null);
        sut.SetupAuthenticatedUser();

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveLastStartDate_AndCourseIsNotOfLearningType_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] RemoveAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCourse(CourseType.ShortCourse));
        sut.SetupAuthenticatedUser();

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupUrlHelper(RemoveAllowedCourseLastStartDateController sut)
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
        var title = routeLearningType == RouteLearningType.ApprenticeshipUnit ? UnitCourseTitle : CourseTitle;
        var level = routeLearningType == RouteLearningType.ApprenticeshipUnit ? UnitCourseLevel : CourseLevel;
        ukprnServiceMock.SetupOrganisationRestriction(Ukprn, isRestricted: true, courseType, ProviderName);
        SetupAllowedCourse(outerApiClientMock, allowedCourse: CreateAllowedCourse(LastDateStarts, title, level));
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCourse(courseType, title, level));
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

    private static ProviderAllowedCourseModel CreateAllowedCourse(
        DateTime? lastDateStarts = null,
        string title = CourseTitle,
        int level = CourseLevel)
        => new()
        {
            LarsCode = LarsCode,
            Title = title,
            Level = level,
            LastDateStarts = lastDateStarts,
            IsClosedToNewStarts = false
        };

    private static GetCourseResponse CreateCourse(
        CourseType courseType,
        string title = CourseTitle,
        int level = CourseLevel)
        => new()
        {
            LarsCode = LarsCode,
            Title = title,
            Level = level,
            CourseType = courseType
        };

    private static bool HasRouteValue(UrlRouteContext context, string propertyName, object expected)
        => Equals(context.Values?.GetType().GetProperty(propertyName)?.GetValue(context.Values), expected);
}

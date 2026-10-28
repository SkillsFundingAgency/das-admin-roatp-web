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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageRestrictedProvider.ChangeAllowedCourseLastStartDateControllerTests;

[TestFixture]
public class ChangeAllowedCourseLastStartDateControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string CourseTitle = "Carpentry";
    private const int CourseLevel = 1;
    private const string UnitCourseTitle = "Permanent modular building assembly - Apprenticeship unit";
    private const int UnitCourseLevel = 4;
    private const string ProviderName = "Denton Business Services Limited";
    private const string AllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";
    private static readonly DateTime LastDateStarts = new(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastStartDate_AndProviderIsRestricted_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new CancellationToken()) as ViewResult;
        var model = result?.Model as ChangeAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.RouteLearningType.Should().Be(RouteLearningType.Apprenticeship);
            model.LarsCode.Should().Be(LarsCode);
            model.CourseDisplayTitle.Should().Be("Carpentry (Level 1)");
            model.LastDateStarts.Should().Be(LastDateStarts);
            model.LastDateStartsText.Should().Be("12 Jul 2026");
            model.SelectedOption.Should().BeNull();
            model.CancelUrl.Should().Be(AllowedCoursesUrl);
            model.Should().NotBeAssignableTo<IBackLink>();
        }

        outerApiClientMock.Verify(
            c => c.GetProviderAllowedCourse(Ukprn, LarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        coursesServiceMock.Verify(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastStartDate_AndAllowedCourseHasNoLastDateStarts_ThenRedirectsToAddLastStartDate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock, allowedCourse: CreateAllowedCourse());

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new CancellationToken()) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.AddAllowedCourseLastStartDate);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["routeLearningType"].Should().Be(RouteLearningType.Apprenticeship);
            result.RouteValues["larsCode"].Should().Be(LarsCode);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastStartDate_AndRouteLearningTypeIsApprenticeshipUnit_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
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
        var model = result?.Model as ChangeAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.LarsCode.Should().Be(LarsCode);
            model.RouteLearningType.Should().Be(RouteLearningType.ApprenticeshipUnit);
            model.CourseDisplayTitle.Should().Be("Permanent modular building assembly - Apprenticeship unit (Level 4)");
            model.LastDateStarts.Should().Be(LastDateStarts);
            model.LastDateStartsText.Should().Be("12 Jul 2026");
            model.SelectedOption.Should().BeNull();
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
    public async Task WhenGettingChangeLastStartDate_AndRouteLearningTypeIsInvalid_ThenReturnsNotFound(
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        var result = await sut.Index(Ukprn, (RouteLearningType)0, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastStartDate_AndRouteLearningTypeIsApprenticeshipAndProviderIsUnrestricted_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
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
    public async Task WhenGettingChangeLastStartDate_AndRouteLearningTypeIsApprenticeshipUnitAndProviderIsNotRestricted_ThenReturnsView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
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
        var model = result?.Model as ChangeAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.RouteLearningType.Should().Be(RouteLearningType.ApprenticeshipUnit);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastStartDate_AndOrganisationIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastStartDate_AndAllowedCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock, HttpStatusCode.NotFound);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
        coursesServiceMock.Verify(c => c.GetCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastStartDate_AndAllowedCourseContentIsNull_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastStartDate_AndApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        SetupAllowedCourse(outerApiClientMock, HttpStatusCode.InternalServerError);

        var act = () => sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        await act.Should().ThrowAsync<ApiException>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastStartDate_AndCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCourseResponse?)null);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastStartDate_AndCourseIsNotOfLearningType_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, coursesServiceMock);
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCourse(CourseType.ShortCourse));

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, LarsCode, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public void WhenExposingOptions_ThenReturnsExpectedValues()
    {
        using (new AssertionScope())
        {
            ChangeAllowedCourseLastStartDateOptions.Change.Should().Be("Change");
            ChangeAllowedCourseLastStartDateOptions.Remove.Should().Be("Remove");
        }
    }

    private static void SetupUrlHelper(ChangeAllowedCourseLastStartDateController sut)
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

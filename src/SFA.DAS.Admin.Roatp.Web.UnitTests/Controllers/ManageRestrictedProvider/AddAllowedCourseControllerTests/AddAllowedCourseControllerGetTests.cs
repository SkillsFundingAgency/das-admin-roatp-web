using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageRestrictedProvider.AddAllowedCourseControllerTests;

[TestFixture]
public class AddAllowedCourseControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string ProviderName = "Denton Business Services Limited";
    private const string AllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";

    [Test, MoqAutoData]
    public async Task WhenGettingAddAllowedCourse_AndProviderIsRestricted_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddAllowedCourseController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, CreateCoursesResponse());
        SetupUrlHelper(sut);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken()) as ViewResult;
        var model = result?.Model as AddAllowedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddAllowedCourseController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.ProviderName.Should().Be(ProviderName);
            model.CancelUrl.Should().Be(AllowedCoursesUrl);
            model.RouteLearningType.Should().Be(RouteLearningType.Apprenticeship);
            model.IsApprenticeship.Should().BeTrue();
            model.IsApprenticeshipUnit.Should().BeFalse();
            model.Courses.Select(course => course.Value).Should().Equal("105", "200");
            model.Should().NotBeAssignableTo<IBackLink>();
            model.Courses.Select(course => course.Text).Should().Equal(
                "Alpha course (Level 6)",
                "Zebra course (Level 3)");
        }

        outerApiClientMock.Verify(
            c => c.GetCoursesToBeAllowed(Ukprn, CourseType.Apprenticeship, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddAllowedCourse_AndRouteLearningTypeIsApprenticeshipUnit_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddAllowedCourseController sut)
    {
        const string allowedUnitsUrl = "/providers/10019900/ApprenticeshipUnit/allowed-courses";
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            CreateCoursesResponse(),
            RouteLearningType.ApprenticeshipUnit);
        var urlHelperMock = sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, allowedUnitsUrl);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            new CancellationToken()) as ViewResult;
        var model = result?.Model as AddAllowedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddAllowedCourseController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.RouteLearningType.Should().Be(RouteLearningType.ApprenticeshipUnit);
            model.IsApprenticeshipUnit.Should().BeTrue();
            model.IsApprenticeship.Should().BeFalse();
            model.CancelUrl.Should().Be(allowedUnitsUrl);
            model.Should().NotBeAssignableTo<IBackLink>();
            model.Courses.Select(course => course.Text).Should().Equal(
                "Alpha course (Level 6)",
                "Zebra course (Level 3)");
        }

        outerApiClientMock.Verify(
            c => c.GetCoursesToBeAllowed(Ukprn, CourseType.ShortCourse, It.IsAny<CancellationToken>()),
            Times.Once);
        urlHelperMock.Verify(
            m => m.RouteUrl(It.Is<UrlRouteContext>(c =>
                c.RouteName == RouteNames.ProviderAllowedCourses
                && HasRouteValue(c, "ukprn", Ukprn)
                && HasRouteValue(c, "routeLearningType", RouteLearningType.ApprenticeshipUnit))),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddAllowedCourse_AndRouteLearningTypeIsInvalid_ThenReturnsNotFound(
        [Greedy] AddAllowedCourseController sut)
    {
        var result = await sut.Index(Ukprn, (RouteLearningType)0, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddAllowedCourse_AndRouteLearningTypeIsApprenticeshipAndProviderIsUnrestricted_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddAllowedCourseController sut)
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
            c => c.GetCoursesToBeAllowed(It.IsAny<int>(), It.IsAny<CourseType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddAllowedCourse_AndRouteLearningTypeIsApprenticeshipUnitAndProviderIsNotRestricted_ThenReturnsView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddAllowedCourseController sut)
    {
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            CreateCoursesResponse(),
            RouteLearningType.ApprenticeshipUnit,
            isRestricted: false);
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            new CancellationToken()) as ViewResult;
        var model = result?.Model as AddAllowedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddAllowedCourseController.ViewPath);
            model.Should().NotBeNull();
            model!.RouteLearningType.Should().Be(RouteLearningType.ApprenticeshipUnit);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddAllowedCourse_AndOrganisationIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddAllowedCourseController sut)
    {
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddAllowedCourse_AndCoursesAreEmpty_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddAllowedCourseController sut)
    {
        SetupRestrictedProvider(
            ukprnServiceMock,
            outerApiClientMock,
            new GetCoursesToBeAllowedResponse { Courses = [] });

        var result = await sut.Index(Ukprn, RouteLearningType.Apprenticeship, new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupUrlHelper(AddAllowedCourseController sut)
    {
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, AllowedCoursesUrl);
    }

    private static void SetupRestrictedProvider(
        Mock<IUkprnService> ukprnServiceMock,
        Mock<IOuterApiClient> outerApiClientMock,
        GetCoursesToBeAllowedResponse? response = null,
        RouteLearningType routeLearningType = RouteLearningType.Apprenticeship,
        bool isRestricted = true)
    {
        var courseType = routeLearningType.ToCourseType();
        ukprnServiceMock.SetupOrganisationRestriction(Ukprn, isRestricted, courseType, ProviderName);
        outerApiClientMock
            .Setup(c => c.GetCoursesToBeAllowed(Ukprn, courseType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response ?? new GetCoursesToBeAllowedResponse());
    }

    private static GetCoursesToBeAllowedResponse CreateCoursesResponse()
        => new()
        {
            Courses =
            [
                new CourseToBeAllowedModel
                {
                    LarsCode = "200",
                    Title = "Zebra course",
                    Level = 3
                },
                new CourseToBeAllowedModel
                {
                    LarsCode = "105",
                    Title = "Alpha course",
                    Level = 6
                }
            ]
        };

    private static bool HasRouteValue(UrlRouteContext context, string propertyName, object expected)
        => Equals(context.Values?.GetType().GetProperty(propertyName)?.GetValue(context.Values), expected);
}

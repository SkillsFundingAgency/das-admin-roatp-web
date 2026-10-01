using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ChangeRestrictedCourseLastStartDateControllerTests;

[TestFixture]
public class ChangeRestrictedCourseLastStartDateControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string DisplayTitle = "Electrical (Level 3)";
    private const string ProviderName = "Denton Business Services Limited";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";
    private static readonly DateTime LastDateStarts = new(2026, 8, 2, 0, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime CourseLastDateStarts = new(2028, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndLastStartDateExists_ThenPrepopulatesDateFields(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeRestrictedCourseLastStartDateModel>> eligibilityValidatorMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock, LastDateStarts);
        SetupEligibility(eligibilityValidatorMock);
        SetupCourseLastDateStarts(outerApiClientMock);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as ViewResult;
        var model = result?.Model as ChangeRestrictedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeRestrictedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.LarsCode.Should().Be(LarsCode);
            model.CourseDisplayTitle.Should().Be(DisplayTitle);
            model.Day.Should().Be("02");
            model.Month.Should().Be("08");
            model.Year.Should().Be("2026");
            model.CourseLastDateStarts.Should().Be(CourseLastDateStarts);
            model.CancelUrl.Should().Be(RestrictedCoursesUrl);
        }

        eligibilityValidatorMock.Verify(
            v => v.ValidateAsync(
                It.Is<ChangeRestrictedCourseLastStartDateModel>(m =>
                    m.Ukprn == Ukprn
                    && m.LarsCode == LarsCode
                    && m.LastDateStarts == LastDateStarts),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndLastStartDateDoesNotExist_ThenDateFieldsAreBlank(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeRestrictedCourseLastStartDateModel>> eligibilityValidatorMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock, lastDateStarts: null);
        SetupEligibility(eligibilityValidatorMock);
        SetupCourseLastDateStarts(outerApiClientMock);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as ViewResult;
        var model = result?.Model as ChangeRestrictedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeRestrictedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.CourseDisplayTitle.Should().Be(DisplayTitle);
            model.Day.Should().BeNull();
            model.Month.Should().BeNull();
            model.Year.Should().BeNull();
            model.CancelUrl.Should().Be(RestrictedCoursesUrl);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndSessionIsMissing_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<ChangeRestrictedCourseLastStartDateModel>> eligibilityValidatorMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<ChangeRestrictedCourseRestrictionSessionModel>(SessionKeys.ProviderRestrictedCourseChangeRestriction))
            .Returns((ChangeRestrictedCourseRestrictionSessionModel?)null);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        eligibilityValidatorMock.Verify(
            v => v.ValidateAsync(
                It.IsAny<ChangeRestrictedCourseLastStartDateModel>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndSessionUkprnDoesNotMatch_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock, LastDateStarts, ukprn: 99999999);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndSessionLarsCodeDoesNotMatch_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock, LastDateStarts, larsCode: "999");

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndEligibilityIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<ChangeRestrictedCourseLastStartDateModel>> eligibilityValidatorMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock, LastDateStarts);
        SetupEligibility(eligibilityValidatorMock, isValid: false);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndCourseLastDateStartsApiReturnsNotFound_ThenReturnsViewWithNullCourseLastDateStarts(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeRestrictedCourseLastStartDateModel>> eligibilityValidatorMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock, lastDateStarts: null);
        SetupEligibility(eligibilityValidatorMock);
        SetupCourseLastDateStarts(outerApiClientMock, HttpStatusCode.NotFound);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as ViewResult;
        var model = result?.Model as ChangeRestrictedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeRestrictedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.CourseLastDateStarts.Should().BeNull();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingSetLastStartDate_AndCourseLastDateStartsApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeRestrictedCourseLastStartDateModel>> eligibilityValidatorMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock, lastDateStarts: null);
        SetupEligibility(eligibilityValidatorMock);
        SetupCourseLastDateStarts(outerApiClientMock, HttpStatusCode.InternalServerError);

        var act = () => sut.Index(Ukprn, LarsCode, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
    }

    private static void SetupSession(
        Mock<ISessionService> sessionServiceMock,
        DateTime? lastDateStarts,
        int ukprn = Ukprn,
        string larsCode = LarsCode)
    {
        sessionServiceMock
            .Setup(s => s.Get<ChangeRestrictedCourseRestrictionSessionModel>(SessionKeys.ProviderRestrictedCourseChangeRestriction))
            .Returns(new ChangeRestrictedCourseRestrictionSessionModel
            {
                Ukprn = ukprn,
                LarsCode = larsCode,
                CourseDisplayTitle = DisplayTitle,
                ProviderName = ProviderName,
                LastDateStarts = lastDateStarts
            });
    }

    private static void SetupEligibility(
        Mock<IValidator<ChangeRestrictedCourseLastStartDateModel>> eligibilityValidatorMock,
        bool isValid = true)
    {
        eligibilityValidatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ChangeRestrictedCourseLastStartDateModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isValid
                ? new ValidationResult()
                : new ValidationResult([new ValidationFailure("Ukprn", "invalid")]));
    }

    private static void SetupCourseLastDateStarts(
        Mock<IOuterApiClient> outerApiClientMock,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        ApiException? apiException = null;
        if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NotFound)
        {
            apiException = ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Get,
                httpResponse,
                new RefitSettings()).GetAwaiter().GetResult();
        }

        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                httpResponse,
                new GetRestrictedCourseDetailsResponse
                {
                    LarsCode = LarsCode,
                    IfateReferenceNumber = "ST0001",
                    CourseName = "Electrical",
                    Route = "Construction",
                    LastDateStarts = CourseLastDateStarts
                },
                new RefitSettings(),
                apiException));
    }
}

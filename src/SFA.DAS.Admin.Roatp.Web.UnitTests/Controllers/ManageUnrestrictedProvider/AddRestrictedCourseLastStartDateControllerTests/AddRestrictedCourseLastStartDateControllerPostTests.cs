using System.Net;
using System.Security.Claims;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.AddRestrictedCourseLastStartDateControllerTests;

[TestFixture]
public class AddRestrictedCourseLastStartDateControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string DisplayTitle = "Electrical (Level 3)";
    private const string ProviderName = "Denton Business Services Limited";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";
    private static readonly DateTime CourseLastDateStarts = new(2028, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime EnteredDate = new(2027, 7, 12, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndDateIsValid_ThenUpsertsCourseSetsBannerAndRedirectsToList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] AddRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock);
        SetupAuthenticatedUser(sut);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        SetupUpsertResponse(outerApiClientMock, HttpStatusCode.OK);

        var result = await sut.Index(
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2027" },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            sut.TempData[ProviderRestrictedApprenticeshipsController.SuccessBannerTempDataKey]
                .Should().Be(AddRestrictedCourseLastStartDateController.GetSuccessBannerMessage(DisplayTitle));
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.ProviderRestrictedCourse), Times.Once);
        outerApiClientMock.Verify(
            c => c.GetAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        validatorMock.Verify(
            v => v.ValidateAsync(
                It.Is<SetLastDateStartsSubmitModel>(m =>
                    m.LarsCode == LarsCode
                    && m.CourseLastDateStarts == CourseLastDateStarts),
                It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            Ukprn,
            LarsCode,
            It.Is<UpsertProviderAllowedCourseRequest>(r =>
                r.UserId == "TestUser@education.gov.uk"
                && r.UserDisplayName == "Test User"
                && r.LastDateStarts == EnteredDate
                && r.IsStartRestricted == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndNoDateIsEntered_ThenReloadsViewWithError(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] AddRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock);
        sut.AddTempData();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(SetLastDateStartsSubmitModel.Day),
                    SetLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage)
            ]));

        var result = await sut.Index(
            Ukprn,
            new SetLastDateStartsSubmitModel(),
            CancellationToken.None) as ViewResult;
        var model = result?.Model as AddRestrictedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddRestrictedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.CourseDisplayTitle.Should().Be(DisplayTitle);
            model.CourseLastDateStarts.Should().Be(CourseLastDateStarts);
            sut.ModelState.IsValid.Should().BeFalse();
        }

        outerApiClientMock.Verify(
            c => c.GetAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<UpsertProviderAllowedCourseRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndDateIsAfterCourseLastDateStarts_ThenReloadsViewWithLarsError(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock)
    {
        SetupSession(sessionServiceMock);
        var sut = new AddRestrictedCourseLastStartDateController(
            sessionServiceMock.Object,
            outerApiClientMock.Object,
            new SetLastDateStartsSubmitModelValidator());
        sut.AddTempData();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "02", Month = "06", Year = "2028" },
            CancellationToken.None) as ViewResult;
        var model = result?.Model as AddRestrictedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddRestrictedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.Day.Should().Be("02");
            model.Month.Should().Be("06");
            model.Year.Should().Be("2028");
            sut.ModelState.IsValid.Should().BeFalse();
            sut.ModelState[SetLastDateStartsSubmitModelValidator.DateFieldName]!
                .Errors.Should().ContainSingle(e =>
                    e.ErrorMessage ==
                    $"The latest start date for this course is {CourseLastDateStarts.ToDisplayString()}. It is set by LARS and cannot be changed. Your chosen last date for new starts must come on or before this.");
        }

        outerApiClientMock.Verify(
            c => c.GetAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<UpsertProviderAllowedCourseRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndDateIsBeforeMinimum_ThenReloadsViewWithMinimumDateError(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock)
    {
        SetupSession(sessionServiceMock);
        var sut = new AddRestrictedCourseLastStartDateController(
            sessionServiceMock.Object,
            outerApiClientMock.Object,
            new SetLastDateStartsSubmitModelValidator());
        sut.AddTempData();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "31", Month = "08", Year = "2014" },
            CancellationToken.None) as ViewResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddRestrictedCourseLastStartDateController.ViewPath);
            sut.ModelState.IsValid.Should().BeFalse();
            sut.ModelState[SetLastDateStartsSubmitModelValidator.DateFieldName]!
                .Errors.Should().ContainSingle(e =>
                    e.ErrorMessage == SetLastDateStartsSubmitModelValidator.DateMustBeAfterMinimumErrorMessage);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndSessionIsMissing_ThenRedirectsWithoutCallingApi(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] AddRestrictedCourseLastStartDateController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<ProviderRestrictedCourseSessionModel>(SessionKeys.ProviderRestrictedCourse))
            .Returns((ProviderRestrictedCourseSessionModel?)null);

        var result = await sut.Index(
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2027" },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<UpsertProviderAllowedCourseRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndSessionUkprnDoesNotMatch_ThenRedirectsWithoutCallingApi(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] AddRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock, ukprn: 99999999);

        var result = await sut.Index(
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2027" },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<UpsertProviderAllowedCourseRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task WhenPostingSetLastStartDate_AndApiReturnsUnexpectedError_ThenThrows(HttpStatusCode statusCode)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        var outerApiClientMock = new Mock<IOuterApiClient>();
        var validatorMock = new Mock<IValidator<SetLastDateStartsSubmitModel>>();
        SetupSession(sessionServiceMock);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        SetupUpsertResponse(outerApiClientMock, statusCode);

        var sut = new AddRestrictedCourseLastStartDateController(
            sessionServiceMock.Object,
            outerApiClientMock.Object,
            validatorMock.Object);
        SetupAuthenticatedUser(sut);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var act = () => sut.Index(
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2027" },
            CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.ProviderRestrictedCourse), Times.Never);
    }

    private static void SetupSession(Mock<ISessionService> sessionServiceMock, int ukprn = Ukprn)
    {
        sessionServiceMock
            .Setup(s => s.Get<ProviderRestrictedCourseSessionModel>(SessionKeys.ProviderRestrictedCourse))
            .Returns(new ProviderRestrictedCourseSessionModel
            {
                Ukprn = ukprn,
                LarsCode = LarsCode,
                CourseDisplayTitle = DisplayTitle,
                ProviderName = ProviderName,
                CourseLastDateStarts = CourseLastDateStarts
            });
    }

    private static void SetupAuthenticatedUser(AddRestrictedCourseLastStartDateController sut)
    {
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn", "TestUser@education.gov.uk")
                ], "test"))
            }
        };
        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
    }

    private static void SetupUpsertResponse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        ApiException? apiException = null;
        if (statusCode != HttpStatusCode.OK)
        {
            apiException = ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Post,
                httpResponse,
                new RefitSettings()).GetAwaiter().GetResult();
        }

        outerApiClientMock
            .Setup(c => c.UpsertProviderAllowedCourse(
                Ukprn,
                LarsCode,
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<object>(
                httpResponse,
                null,
                new RefitSettings(),
                apiException));
    }
}

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
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ChangeRestrictedCourseLastStartDateControllerTests;

[TestFixture]
public class ChangeRestrictedCourseLastStartDateControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string DisplayTitle = "Electrical (Level 3)";
    private const string ProviderName = "Denton Business Services Limited";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";
    private static readonly DateTime CourseLastDateStarts = new(2028, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime EnteredDate = new(2026, 8, 2, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndDateIsValid_ThenChangesCourseDeletesSessionSetsBannerAndRedirectsToList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> submitValidatorMock,
        [Frozen] Mock<IValidator<ChangeRestrictedCourseRestrictionSessionModel>> validatorMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock);
        SetupCourseDetails(outerApiClientMock);
        SetupAuthenticatedUser(sut);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);
        submitValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        SetupChangeResponse(outerApiClientMock, HttpStatusCode.OK);

        var result = await sut.Index(
            Ukprn,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "2", Month = "8", Year = "2026" },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            sut.TempData[ProviderRestrictedApprenticeshipsController.SuccessBannerTempDataKey]
                .Should().Be(ChangeRestrictedCourseLastStartDateController.GetSuccessBannerMessage(DisplayTitle));
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Once);
        validatorMock.Verify(
            v => v.ValidateAsync(
                It.IsAny<ChangeRestrictedCourseRestrictionSessionModel>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        submitValidatorMock.Verify(
            v => v.ValidateAsync(
                It.Is<SetLastDateStartsSubmitModel>(m =>
                    m.LarsCode == LarsCode
                    && m.CourseLastDateStarts == CourseLastDateStarts),
                It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(c => c.ChangeRestrictedApprenticeshipLastDateStarts(
            Ukprn,
            LarsCode,
            It.Is<ChangeRestrictedApprenticeshipLastDateStartsRequest>(r =>
                r.UserId == "TestUser@education.gov.uk"
                && r.UserDisplayName == "Test User"
                && r.LastDateStarts == EnteredDate),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndNoDateIsEntered_ThenReloadsViewWithError(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock);
        SetupCourseDetails(outerApiClientMock);
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
            LarsCode,
            new SetLastDateStartsSubmitModel(),
            CancellationToken.None) as ViewResult;
        var model = result?.Model as ChangeRestrictedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeRestrictedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.CourseDisplayTitle.Should().Be(DisplayTitle);
            sut.ModelState.IsValid.Should().BeFalse();
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Never);
        outerApiClientMock.Verify(c => c.ChangeRestrictedApprenticeshipLastDateStarts(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<ChangeRestrictedApprenticeshipLastDateStartsRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndValidatorPassesButDateIsInvalid_ThenReloadsViewWithError(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock);
        SetupCourseDetails(outerApiClientMock);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var result = await sut.Index(
            Ukprn,
            LarsCode,
            new SetLastDateStartsSubmitModel(),
            CancellationToken.None) as ViewResult;
        var model = result?.Model as ChangeRestrictedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeRestrictedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            sut.ModelState.IsValid.Should().BeFalse();
            sut.ModelState[SetLastDateStartsSubmitModelValidator.DateFieldName]!
                .Errors.Should().ContainSingle(e =>
                    e.ErrorMessage == SetLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage);
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Never);
        outerApiClientMock.Verify(c => c.ChangeRestrictedApprenticeshipLastDateStarts(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<ChangeRestrictedApprenticeshipLastDateStartsRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task WhenPostingSetLastStartDate_AndDateIsBeforeMinimum_ThenReloadsViewWithMinimumDateError()
    {
        var sessionServiceMock = new Mock<ISessionService>();
        var outerApiClientMock = new Mock<IOuterApiClient>();
        var validatorMock = new Mock<IValidator<ChangeRestrictedCourseRestrictionSessionModel>>();
        SetupSession(sessionServiceMock);
        SetupCourseDetails(outerApiClientMock);
        var sut = new ChangeRestrictedCourseLastStartDateController(
            sessionServiceMock.Object,
            outerApiClientMock.Object,
            new SetLastDateStartsSubmitModelValidator(),
            validatorMock.Object);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(
            Ukprn,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "31", Month = "08", Year = "2014" },
            CancellationToken.None) as ViewResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeRestrictedCourseLastStartDateController.ViewPath);
            sut.ModelState.IsValid.Should().BeFalse();
            sut.ModelState[SetLastDateStartsSubmitModelValidator.DateFieldName]!
                .Errors.Should().ContainSingle(e =>
                    e.ErrorMessage == SetLastDateStartsSubmitModelValidator.DateMustBeAfterMinimumErrorMessage);
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Never);
        outerApiClientMock.Verify(c => c.ChangeRestrictedApprenticeshipLastDateStarts(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<ChangeRestrictedApprenticeshipLastDateStartsRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task WhenPostingSetLastStartDate_AndDateIsAfterCourseLastDateStarts_ThenReloadsViewWithLarsError()
    {
        var sessionServiceMock = new Mock<ISessionService>();
        var outerApiClientMock = new Mock<IOuterApiClient>();
        var validatorMock = new Mock<IValidator<ChangeRestrictedCourseRestrictionSessionModel>>();
        SetupSession(sessionServiceMock);
        SetupCourseDetails(outerApiClientMock);
        var sut = new ChangeRestrictedCourseLastStartDateController(
            sessionServiceMock.Object,
            outerApiClientMock.Object,
            new SetLastDateStartsSubmitModelValidator(),
            validatorMock.Object);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(
            Ukprn,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "02", Month = "06", Year = "2028" },
            CancellationToken.None) as ViewResult;
        var model = result?.Model as ChangeRestrictedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeRestrictedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            sut.ModelState.IsValid.Should().BeFalse();
            sut.ModelState[SetLastDateStartsSubmitModelValidator.DateFieldName]!
                .Errors.Should().ContainSingle(e =>
                    e.ErrorMessage ==
                    $"The latest start date for this course is {CourseLastDateStarts.ToDisplayString()}. It is set by LARS and cannot be changed. Your chosen last date for new starts must come on or before this.");
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Never);
        outerApiClientMock.Verify(c => c.ChangeRestrictedApprenticeshipLastDateStarts(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<ChangeRestrictedApprenticeshipLastDateStartsRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndSessionIsMissing_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<ChangeRestrictedCourseRestrictionSessionModel>(SessionKeys.RestrictedCourseChangeRestriction))
            .Returns((ChangeRestrictedCourseRestrictionSessionModel?)null);

        var result = await sut.Index(
            Ukprn,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "2", Month = "8", Year = "2026" },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Never);
        outerApiClientMock.Verify(c => c.ChangeRestrictedApprenticeshipLastDateStarts(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<ChangeRestrictedApprenticeshipLastDateStartsRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndApiReturnsNotFound_ThenThrows(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] ChangeRestrictedCourseLastStartDateController sut)
    {
        SetupSession(sessionServiceMock);
        SetupCourseDetails(outerApiClientMock);
        SetupAuthenticatedUser(sut);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        SetupChangeResponse(outerApiClientMock, HttpStatusCode.NotFound);

        var act = () => sut.Index(
            Ukprn,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "2", Month = "8", Year = "2026" },
            CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Never);
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task WhenPostingSetLastStartDate_AndApiReturnsUnexpectedError_ThenThrows(HttpStatusCode statusCode)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        var outerApiClientMock = new Mock<IOuterApiClient>();
        var submitValidatorMock = new Mock<IValidator<SetLastDateStartsSubmitModel>>();
        var validatorMock = new Mock<IValidator<ChangeRestrictedCourseRestrictionSessionModel>>();
        SetupSession(sessionServiceMock);
        SetupCourseDetails(outerApiClientMock);
        submitValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        SetupChangeResponse(outerApiClientMock, statusCode);

        var sut = new ChangeRestrictedCourseLastStartDateController(
            sessionServiceMock.Object,
            outerApiClientMock.Object,
            submitValidatorMock.Object,
            validatorMock.Object);
        SetupAuthenticatedUser(sut);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var act = () => sut.Index(
            Ukprn,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "2", Month = "8", Year = "2026" },
            CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Never);
    }

    private static void SetupSession(Mock<ISessionService> sessionServiceMock)
    {
        sessionServiceMock
            .Setup(s => s.Get<ChangeRestrictedCourseRestrictionSessionModel>(SessionKeys.RestrictedCourseChangeRestriction))
            .Returns(new ChangeRestrictedCourseRestrictionSessionModel
            {
                Ukprn = Ukprn,
                LarsCode = LarsCode,
                CourseDisplayTitle = DisplayTitle,
                ProviderName = ProviderName,
                CourseLastDateStarts = null
            });
    }

    private static void SetupCourseDetails(Mock<IOuterApiClient> outerApiClientMock)
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
                    LastDateStarts = CourseLastDateStarts,
                    IsCourseRestricted = true
                },
                new RefitSettings(),
                null));
    }

    private static void SetupAuthenticatedUser(ChangeRestrictedCourseLastStartDateController sut)
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

    private static void SetupChangeResponse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
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
            .Setup(c => c.ChangeRestrictedApprenticeshipLastDateStarts(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<ChangeRestrictedApprenticeshipLastDateStartsRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<object>(httpResponse, null, new RefitSettings(), apiException));
    }
}

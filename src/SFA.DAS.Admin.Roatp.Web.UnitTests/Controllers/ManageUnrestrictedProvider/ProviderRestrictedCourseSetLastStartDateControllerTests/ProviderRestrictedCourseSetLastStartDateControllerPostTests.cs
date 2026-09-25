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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ProviderRestrictedCourseSetLastStartDateControllerTests;

[TestFixture]
public class ProviderRestrictedCourseSetLastStartDateControllerPostTests
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
        [Greedy] ProviderRestrictedCourseSetLastStartDateController sut)
    {
        SetupSession(sessionServiceMock);
        SetupCourseLastDateStarts(outerApiClientMock);
        SetupAuthenticatedUser(sut);
        sut.TempData[TempDataKeys.ProviderLegalName] = ProviderName;
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);
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
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            sut.TempData[RestrictedApprenticeshipsController.SuccessBannerTempDataKey]
                .Should().Be(ProviderRestrictedCourseSetLastStartDateController.GetSuccessBannerMessage(DisplayTitle));
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.ProviderRestrictedCourse), Times.Once);
        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            Ukprn,
            LarsCode,
            It.Is<UpsertProviderAllowedCourseRequest>(r =>
                r.UserId == "TestUser@education.gov.uk"
                && r.UserDisplayName == "Test User"
                && r.LastDateStarts == EnteredDate
                && r.IsStartRestricted),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndNoDateIsEntered_ThenReloadsViewWithError(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] ProviderRestrictedCourseSetLastStartDateController sut)
    {
        SetupSession(sessionServiceMock);
        SetupCourseLastDateStarts(outerApiClientMock);
        sut.AddTempData();
        sut.TempData[TempDataKeys.ProviderLegalName] = ProviderName;
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);
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
        var model = result?.Model as ProviderRestrictedCourseSetLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ProviderRestrictedCourseSetLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.DisplayTitle.Should().Be(DisplayTitle);
            sut.ModelState.IsValid.Should().BeFalse();
        }

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
        SetupCourseLastDateStarts(outerApiClientMock);
        var sut = new ProviderRestrictedCourseSetLastStartDateController(
            sessionServiceMock.Object,
            outerApiClientMock.Object,
            new SetLastDateStartsSubmitModelValidator());
        sut.AddTempData();
        sut.TempData[TempDataKeys.ProviderLegalName] = ProviderName;
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "02", Month = "06", Year = "2028" },
            CancellationToken.None) as ViewResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ProviderRestrictedCourseSetLastStartDateController.ViewPath);
            sut.ModelState.IsValid.Should().BeFalse();
            sut.ModelState[SetLastDateStartsSubmitModelValidator.DateFieldName]!
                .Errors.Should().ContainSingle(e =>
                    e.ErrorMessage ==
                    $"The latest start date for this course is {CourseLastDateStarts.ToDisplayString()}. It is set by LARS and cannot be changed. Your chosen last date for new starts must come on or before this.");
        }

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
        SetupCourseLastDateStarts(outerApiClientMock);
        var sut = new ProviderRestrictedCourseSetLastStartDateController(
            sessionServiceMock.Object,
            outerApiClientMock.Object,
            new SetLastDateStartsSubmitModelValidator());
        sut.AddTempData();
        sut.TempData[TempDataKeys.ProviderLegalName] = ProviderName;
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "31", Month = "08", Year = "2014" },
            CancellationToken.None) as ViewResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ProviderRestrictedCourseSetLastStartDateController.ViewPath);
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
        [Greedy] ProviderRestrictedCourseSetLastStartDateController sut)
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
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<UpsertProviderAllowedCourseRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSetLastStartDate_AndApiReturnsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] ProviderRestrictedCourseSetLastStartDateController sut)
    {
        SetupSession(sessionServiceMock);
        SetupCourseLastDateStarts(outerApiClientMock);
        SetupAuthenticatedUser(sut);
        sut.TempData[TempDataKeys.ProviderLegalName] = ProviderName;
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        SetupUpsertResponse(outerApiClientMock, HttpStatusCode.NotFound);

        var result = await sut.Index(
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2027" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.ProviderRestrictedCourse), Times.Never);
    }

    private static void SetupSession(Mock<ISessionService> sessionServiceMock)
    {
        sessionServiceMock
            .Setup(s => s.Get<ProviderRestrictedCourseSessionModel>(SessionKeys.ProviderRestrictedCourse))
            .Returns(new ProviderRestrictedCourseSessionModel
            {
                Ukprn = Ukprn,
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

    private static void SetupAuthenticatedUser(ProviderRestrictedCourseSetLastStartDateController sut)
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
        outerApiClientMock
            .Setup(c => c.UpsertProviderAllowedCourse(
                Ukprn,
                LarsCode,
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<object>(
                new HttpResponseMessage(statusCode),
                null,
                new RefitSettings(),
                null));
    }
}

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
using SFA.DAS.Admin.Roatp.Web.Controllers.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.CourseRestrictions.SetLastDateStartsControllerTests;

[TestFixture]
public class SetLastDateStartsControllerPostTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";
    private const string ProviderName = "BP TRAINING";
    private const string CourseDisplayTitle = "Academic professional (Level 7)";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";

    [Test, MoqAutoData]
    public async Task WhenValidationFails_ThenReturnsViewWithErrors(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] SetLastDateStartsController sut)
    {
        SetupSession(sessionServiceMock);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(SetLastDateStartsSubmitModel.Day), "Enter a valid date")
            ]));

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "", Month = "", Year = "" },
            CancellationToken.None) as ViewResult;
        var model = result!.Model as SetLastDateStartsViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            sut.ModelState.IsValid.Should().BeFalse();
            model!.ProviderName.Should().Be(ProviderName);
            model.CourseDisplayTitle.Should().Be(CourseDisplayTitle);
        }

        validatorMock.Verify(
            v => v.ValidateAsync(
                It.Is<SetLastDateStartsSubmitModel>(m => m.LarsCode == LarsCode),
                It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingSubmitDateIsAfterCourseLastDateStarts_ThenReturnsViewWithValidationError(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock)
    {
        var courseLastDateStarts = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);
        SetupSession(sessionServiceMock, courseLastDateStarts);
        var sut = new SetLastDateStartsController(
            outerApiClientMock.Object,
            sessionServiceMock.Object,
            new SetLastDateStartsSubmitModelValidator());
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "02", Month = "06", Year = "2027" },
            CancellationToken.None) as ViewResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(SetLastDateStartsController.ViewPath);
            sut.ModelState.IsValid.Should().BeFalse();
            sut.ModelState[SetLastDateStartsSubmitModelValidator.DateFieldName]!
                .Errors.Should().ContainSingle(e =>
                    e.ErrorMessage ==
                    $"The latest start date for this course is {courseLastDateStarts.ToDisplayString()}. It is set by LARS and cannot be changed. Your chosen last date for new starts must come on or before this.");
        }

        outerApiClientMock.Verify(
            c => c.GetAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.PatchProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<PatchProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenValidationPasses_ThenSetsCourseLastDateStartsAndRedirectsWithSuccessBanner(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] SetLastDateStartsController sut)
    {
        var courseLastDateStarts = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);
        SetupSession(sessionServiceMock, courseLastDateStarts);
        SetupSuccessfulPatch(outerApiClientMock);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);
        SetupTestUser(sut);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "15", Month = "03", Year = "2027" },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.RestrictedCourseDetails);
            result.RouteValues!["larsCode"].Should().Be(LarsCode);
            sut.TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey]
                .Should().Be("Last start date added for BP TRAINING");
        }

        validatorMock.Verify(
            v => v.ValidateAsync(
                It.Is<SetLastDateStartsSubmitModel>(m =>
                    m.LarsCode == LarsCode
                    && m.CourseLastDateStarts == courseLastDateStarts),
                It.IsAny<CancellationToken>()),
            Times.Once);
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.SetLastDateStarts), Times.Once);
        outerApiClientMock.Verify(
            c => c.GetAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(c => c.PatchProviderAllowedCourse(
            Ukprn,
            LarsCode,
            "test.user@education.gov.uk",
            "Test User",
            It.Is<PatchProviderAllowedCourseRequest>(r =>
                r.LastDateStarts == new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Unspecified)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenChangingExistingDate_ThenCallsApiAndRedirectsWithUpdatedBanner(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] SetLastDateStartsController sut)
    {
        SetupSession(
            sessionServiceMock,
            providerLastDateStarts: new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified));
        SetupSuccessfulPatch(outerApiClientMock);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);
        SetupTestUser(sut);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2026" },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.RestrictedCourseDetails);
            sut.TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey]
                .Should().Be("BP TRAINING last start date has been updated");
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.SetLastDateStarts), Times.Once);
        outerApiClientMock.Verify(
            c => c.GetAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(c => c.PatchProviderAllowedCourse(
            Ukprn,
            LarsCode,
            "test.user@education.gov.uk",
            "Test User",
            It.Is<PatchProviderAllowedCourseRequest>(r =>
                r.LastDateStarts == new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task WhenApiReturnsNotFound_ThenReturnsNotFound()
    {
        var outerApiClientMock = new Mock<IOuterApiClient>();
        var sessionServiceMock = new Mock<ISessionService>();
        var validatorMock = new Mock<IValidator<SetLastDateStartsSubmitModel>>();
        SetupSession(sessionServiceMock);
        SetupPatchResponse(outerApiClientMock, HttpStatusCode.NotFound);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var sut = new SetLastDateStartsController(
            outerApiClientMock.Object,
            sessionServiceMock.Object,
            validatorMock.Object);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);
        SetupTestUser(sut);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "15", Month = "03", Year = "2027" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.SetLastDateStarts), Times.Never);
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task WhenApiReturnsUnexpectedError_ThenThrows(HttpStatusCode statusCode)
    {
        var outerApiClientMock = new Mock<IOuterApiClient>();
        var sessionServiceMock = new Mock<ISessionService>();
        var validatorMock = new Mock<IValidator<SetLastDateStartsSubmitModel>>();
        SetupSession(sessionServiceMock);
        SetupPatchResponse(outerApiClientMock, statusCode);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var sut = new SetLastDateStartsController(
            outerApiClientMock.Object,
            sessionServiceMock.Object,
            validatorMock.Object);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);
        SetupTestUser(sut);

        var act = () => sut.Index(
            LarsCode,
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "15", Month = "03", Year = "2027" },
            CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.SetLastDateStarts), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenSessionIsMissing_ThenReturnsNotFound(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] SetLastDateStartsController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<SetLastDateStartsSessionModel>(SessionKeys.SetLastDateStarts))
            .Returns((SetLastDateStartsSessionModel?)null);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "15", Month = "03", Year = "2027" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        outerApiClientMock.Verify(
            c => c.GetAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.PatchProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<PatchProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static void SetupSession(
        Mock<ISessionService> sessionServiceMock,
        DateTime? courseLastDateStarts = null,
        DateTime? providerLastDateStarts = null)
    {
        sessionServiceMock
            .Setup(s => s.Get<SetLastDateStartsSessionModel>(SessionKeys.SetLastDateStarts))
            .Returns(new SetLastDateStartsSessionModel
            {
                Ukprn = Ukprn,
                LarsCode = LarsCode,
                ProviderName = ProviderName,
                CourseDisplayTitle = CourseDisplayTitle,
                CourseLastDateStarts = courseLastDateStarts,
                ProviderLastDateStarts = providerLastDateStarts
            });
    }

    private static void SetupSuccessfulPatch(Mock<IOuterApiClient> outerApiClientMock)
        => SetupPatchResponse(outerApiClientMock, HttpStatusCode.OK);

    private static void SetupPatchResponse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        ApiException? apiException = null;
        if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NotFound)
        {
            apiException = ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Patch,
                httpResponse,
                new RefitSettings()).GetAwaiter().GetResult();
        }

        outerApiClientMock
            .Setup(c => c.PatchProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<PatchProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<object>(httpResponse, null, new RefitSettings(), apiException));
    }

    private static void SetupTestUser(Controller sut)
    {
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn", "test.user@education.gov.uk")
                ], "test"))
            }
        };
        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
    }
}

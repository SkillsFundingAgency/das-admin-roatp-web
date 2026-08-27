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
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.SetLastDateStartsControllerTests;

[TestFixture]
public class SetLastDateStartsControllerPostTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";

    [Test, MoqAutoData]
    public async Task WhenValidationFails_ThenReturnsViewWithErrors(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        var provider = new ProviderCourseModel
        {
            Ukprn = Ukprn,
            ProviderName = "BP TRAINING",
            LastDateStarts = null
        };

        SetupValidRoute(restrictedCourseProviderServiceMock);
        SetupCourseAndProvider(restrictedCourseProviderServiceMock, response, provider);

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(SetLastDateStartsSubmitModel.Day), "Enter a valid date")
            ]));

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var submitModel = new SetLastDateStartsSubmitModel
        {
            Day = "",
            Month = "",
            Year = ""
        };

        var result = await sut.Index(LarsCode, Ukprn, submitModel, CancellationToken.None) as ViewResult;

        var model = result!.Model as SetLastDateStartsViewModel;
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            sut.ModelState.IsValid.Should().BeFalse();
            model!.ProviderName.Should().Be("BP TRAINING");
            model.CourseDisplayTitle.Should().Be("Academic professional (Level 7)");
        }
        validatorMock.Verify(
            v => v.ValidateAsync(
                It.Is<SetLastDateStartsSubmitModel>(m => m.LarsCode == LarsCode),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenValidationPasses_ThenCallsApiAndRedirectsWithSuccessBanner(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        var provider = new ProviderCourseModel
        {
            Ukprn = Ukprn,
            ProviderName = "BP TRAINING",
            LastDateStarts = null
        };

        SetupValidRoute(restrictedCourseProviderServiceMock);
        SetupCourseAndProvider(restrictedCourseProviderServiceMock, response, provider);
        SetupUpsertRequest(restrictedCourseProviderServiceMock);

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        SetupTestUser(sut);

        var submitModel = new SetLastDateStartsSubmitModel
        {
            Day = "15",
            Month = "03",
            Year = "2027"
        };

        var result = await sut.Index(LarsCode, Ukprn, submitModel, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.RestrictedCourseDetails);
            result.RouteValues!["larsCode"].Should().Be(LarsCode);
            sut.TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey]
                .Should().Be("Last start date added for BP TRAINING");
        }

        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            Ukprn,
            LarsCode,
            It.Is<UpsertProviderAllowedCourseRequest>(r =>
                r.LastDateStarts == new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Unspecified)
                && r.UserId == "test.user@education.gov.uk"
                && r.UserDisplayName == "Test User"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenChangingExistingDate_ThenCallsApiAndRedirectsWithUpdatedBanner(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        var provider = new ProviderCourseModel
        {
            Ukprn = Ukprn,
            ProviderName = "BP TRAINING",
            LastDateStarts = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified)
        };

        SetupValidRoute(restrictedCourseProviderServiceMock);
        SetupCourseAndProvider(restrictedCourseProviderServiceMock, response, provider);
        SetupUpsertRequest(restrictedCourseProviderServiceMock);

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

        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            Ukprn,
            LarsCode,
            It.Is<UpsertProviderAllowedCourseRequest>(r =>
                r.LastDateStarts == new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified)
                && r.UserId == "test.user@education.gov.uk"
                && r.UserDisplayName == "Test User"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenRouteIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] SetLastDateStartsController sut)
    {
        restrictedCourseProviderServiceMock
            .Setup(s => s.IsRouteValidAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "15", Month = "03", Year = "2027" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        restrictedCourseProviderServiceMock.Verify(
            s => s.GetCourseAndProviderAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenProviderDoesNotExistOnCourse_ThenReturnsNotFound(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] SetLastDateStartsController sut)
    {
        SetupValidRoute(restrictedCourseProviderServiceMock);
        restrictedCourseProviderServiceMock
            .Setup(s => s.GetCourseAndProviderAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((GetRestrictedCourseDetailsResponse, ProviderCourseModel)?)null);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new SetLastDateStartsSubmitModel { Day = "15", Month = "03", Year = "2027" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupValidRoute(Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock)
    {
        restrictedCourseProviderServiceMock
            .Setup(s => s.IsRouteValidAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private static void SetupCourseAndProvider(
        Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        GetRestrictedCourseDetailsResponse response,
        ProviderCourseModel provider)
    {
        restrictedCourseProviderServiceMock
            .Setup(s => s.GetCourseAndProviderAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((response, provider));
    }

    private static void SetupUpsertRequest(Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock)
    {
        restrictedCourseProviderServiceMock
            .Setup(s => s.CreateUpsertRequest(It.IsAny<ClaimsPrincipal>(), It.IsAny<DateTime?>()))
            .Returns((ClaimsPrincipal user, DateTime? lastDateStarts) => new UpsertProviderAllowedCourseRequest
            {
                UserId = user.UserId(),
                UserDisplayName = user.UserDisplayName(),
                LastDateStarts = lastDateStarts
            });
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

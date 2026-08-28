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
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Admin.Roatp.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.ChangeCourseRestrictionControllerTests;

[TestFixture]
public class ChangeCourseRestrictionControllerPostTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";
    private static readonly DateTime LastDateStarts = new(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenValidationFails_ThenReturnsViewWithErrors(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IValidator<ChangeCourseRestrictionSubmitModel>> validatorMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupCourseWithLastDateStarts(larsCodeServiceMock, response);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeCourseRestrictionSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(ChangeCourseRestrictionSubmitModel.SelectedOption),
                    ChangeCourseRestrictionSubmitModelValidator.NoOptionSelectedErrorMessage)
            ]));

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeCourseRestrictionSubmitModel(),
            CancellationToken.None) as ViewResult;

        var model = result!.Model as ChangeCourseRestrictionViewModel;
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            sut.ModelState.IsValid.Should().BeFalse();
            model!.ProviderName.Should().Be("BP TRAINING");
        }
    }

    [Test, MoqAutoData]
    public async Task WhenChangeSelected_ThenRedirectsToSetLastStartDatePage(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeCourseRestrictionSubmitModel>> validatorMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupCourseWithLastDateStarts(larsCodeServiceMock, response);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeCourseRestrictionSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl)
            .AddUrlForRoute(RouteNames.SetLastDateStarts, "/restricted-courses/105/providers/10007938/set-last-start-date");

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeCourseRestrictionSubmitModel { SelectedOption = ChangeCourseRestrictionOptions.Change },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.SetLastDateStarts);
            result.RouteValues!["larsCode"].Should().Be(LarsCode);
            result.RouteValues["ukprn"].Should().Be(Ukprn);
        }
        outerApiClientMock.Verify(
            c => c.UpsertProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenRemoveSelected_ThenClearsLastDateStartsAndRedirectsWithSuccessBanner(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeCourseRestrictionSubmitModel>> validatorMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupCourseWithLastDateStarts(larsCodeServiceMock, response);
        SetupSuccessfulUpsert(outerApiClientMock);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeCourseRestrictionSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        SetupTestUser(sut);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeCourseRestrictionSubmitModel { SelectedOption = ChangeCourseRestrictionOptions.Remove },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.RestrictedCourseDetails);
            result.RouteValues!["larsCode"].Should().Be(LarsCode);
            sut.TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey]
                .Should().Be("BP TRAINING last start date has been removed");
        }

        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            Ukprn,
            LarsCode,
            It.Is<UpsertProviderAllowedCourseRequest>(r =>
                r.LastDateStarts == null
                && r.UserId == "test.user@education.gov.uk"
                && r.UserDisplayName == "Test User"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.BadRequest)]
    public async Task WhenRemoveSelected_AndApiReturnsNotFoundOrBadRequest_ThenReturnsNotFound(
        HttpStatusCode statusCode)
    {
        var larsCodeServiceMock = new Mock<ILarsCodeService>();
        var outerApiClientMock = new Mock<IOuterApiClient>();
        var validatorMock = new Mock<IValidator<ChangeCourseRestrictionSubmitModel>>();
        var response = new GetRestrictedCourseDetailsResponse
        {
            LarsCode = LarsCode,
            IfateReferenceNumber = "ST0001",
            CourseName = "Academic professional",
            Route = "Education",
            Level = 7
        };

        SetupCourseWithLastDateStarts(larsCodeServiceMock, response);
        SetupUpsertResponse(outerApiClientMock, statusCode);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeCourseRestrictionSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var sut = new ChangeCourseRestrictionController(
            larsCodeServiceMock.Object,
            outerApiClientMock.Object,
            validatorMock.Object);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);
        SetupTestUser(sut);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeCourseRestrictionSubmitModel { SelectedOption = ChangeCourseRestrictionOptions.Remove },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenProviderHasNoLastDateStarts_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel
            {
                Ukprn = Ukprn,
                ProviderName = "BP TRAINING",
                LastDateStarts = null
            }
        ];
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeCourseRestrictionSubmitModel { SelectedOption = ChangeCourseRestrictionOptions.Remove },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupCourseWithLastDateStarts(
        Mock<ILarsCodeService> larsCodeServiceMock,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel
            {
                Ukprn = Ukprn,
                ProviderName = "BP TRAINING",
                LastDateStarts = LastDateStarts
            }
        ];

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    private static void SetupSuccessfulUpsert(Mock<IOuterApiClient> outerApiClientMock)
        => SetupUpsertResponse(outerApiClientMock, HttpStatusCode.OK);

    private static void SetupUpsertResponse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
    {
        var apiResponse = new Mock<IApiResponse>();
        apiResponse.SetupGet(r => r.StatusCode).Returns(statusCode);
        apiResponse.SetupGet(r => r.IsSuccessStatusCode).Returns(statusCode == HttpStatusCode.OK);

        outerApiClientMock
            .Setup(c => c.UpsertProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse.Object);
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

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
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeCourseRestrictionSubmitModel>> validatorMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupCourseWithLastDateStarts(outerApiClientMock, response);
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
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeCourseRestrictionSubmitModel>> validatorMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupCourseWithLastDateStarts(outerApiClientMock, response);
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
    public async Task WhenRemoveSelected_ThenClearsLastDateStartsAndRedirectsWithSuccessBanner(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeCourseRestrictionSubmitModel>> validatorMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupCourseWithLastDateStarts(outerApiClientMock, response);
        SetupSuccessfulPatch(outerApiClientMock);
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

        outerApiClientMock.Verify(c => c.PatchProviderAllowedCourse(
            Ukprn,
            LarsCode,
            "test.user@education.gov.uk",
            "Test User",
            It.Is<PatchProviderAllowedCourseRequest>(r => r.LastDateStarts == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task WhenRemoveSelected_AndApiReturnsNotFound_ThenReturnsNotFound()
    {
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

        SetupCourseWithLastDateStarts(outerApiClientMock, response);
        SetupPatchResponse(outerApiClientMock, HttpStatusCode.NotFound);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeCourseRestrictionSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var sut = new ChangeCourseRestrictionController(
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

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task WhenRemoveSelected_AndApiReturnsUnexpectedError_ThenThrows(HttpStatusCode statusCode)
    {
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

        SetupCourseWithLastDateStarts(outerApiClientMock, response);
        SetupPatchResponse(outerApiClientMock, statusCode);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeCourseRestrictionSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var sut = new ChangeCourseRestrictionController(
            outerApiClientMock.Object,
            validatorMock.Object);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);
        SetupTestUser(sut);

        var act = () => sut.Index(
            LarsCode,
            Ukprn,
            new ChangeCourseRestrictionSubmitModel { SelectedOption = ChangeCourseRestrictionOptions.Remove },
            CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
    }

    [Test, MoqAutoData]
    public async Task WhenProviderHasNoLastDateStarts_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
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
        SetupCourseResponse(outerApiClientMock, response);

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
        Mock<IOuterApiClient> outerApiClientMock,
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

        SetupCourseResponse(outerApiClientMock, response);
    }

    private static void SetupCourseResponse(
        Mock<IOuterApiClient> outerApiClientMock,
        GetRestrictedCourseDetailsResponse response)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));
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

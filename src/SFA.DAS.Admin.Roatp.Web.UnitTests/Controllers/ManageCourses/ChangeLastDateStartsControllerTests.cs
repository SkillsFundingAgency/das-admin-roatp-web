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
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Admin.Roatp.Web.Validators;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class ChangeLastDateStartsControllerTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";
    private static readonly DateTime LastDateStarts = new(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastDateStarts_AndProviderHasLastDateStarts_ThenReturnsView(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] ChangeLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        SetupCourseWithLastDateStarts(response, larsCodeServiceMock);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None) as ViewResult;

        var model = result!.Model as ChangeLastDateStartsViewModel;
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeLastDateStartsController.ViewPath);
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be("BP TRAINING");
            model.Ukprn.Should().Be(Ukprn);
            model.CourseDisplayTitle.Should().Be("Academic professional (Level 7)");
            model.LastDateStarts.Should().Be(LastDateStarts);
            model.LastDateStartsText.Should().Be("01 Jun 2027");
            model.CancelUrl.Should().Be(RestrictedCourseDetailsUrl);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastDateStarts_AndProviderHasNoLastDateStarts_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] ChangeLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = null }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastDateStarts_AndProviderDoesNotExistOnCourse_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] ChangeLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = 10000001, ProviderName = "Other", LastDateStarts = LastDateStarts }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastDateStarts_AndRouteIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] ChangeLastDateStartsController sut)
    {
        ukprnAndLarsCodeValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IUkprnAndLarsCodeValidator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(IUkprnAndLarsCodeValidator.LarsCode), "invalid")
            ]));

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingChangeLastDateStarts_AndCourseDetailsBecomeUnavailable_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] ChangeLastDateStartsController sut)
    {
        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenPosting_AndValidationFails_ThenReturnsViewWithErrors(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IValidator<ChangeLastDateStartsSubmitModel>> validatorMock,
        [Greedy] ChangeLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        SetupCourseWithLastDateStarts(response, larsCodeServiceMock);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(ChangeLastDateStartsSubmitModel.SelectedOption),
                    ChangeLastDateStartsSubmitModelValidator.NoOptionSelectedErrorMessage)
            ]));

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeLastDateStartsSubmitModel(),
            CancellationToken.None) as ViewResult;

        var model = result!.Model as ChangeLastDateStartsViewModel;
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            sut.ModelState.IsValid.Should().BeFalse();
            model!.ProviderName.Should().Be("BP TRAINING");
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPosting_AndChangeLastDateStartsSelected_ThenRedirectsToSetLastStartDatePage(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeLastDateStartsSubmitModel>> validatorMock,
        [Greedy] ChangeLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        SetupCourseWithLastDateStarts(response, larsCodeServiceMock);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl)
            .AddUrlForRoute(RouteNames.AddLastDateStarts, "/restricted-courses/105/providers/10007938/set-last-start-date");

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeLastDateStartsSubmitModel { SelectedOption = ChangeLastDateStartsOptions.Change },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.AddLastDateStarts);
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
    public async Task WhenPosting_AndRemoveLastDateStartsSelected_ThenClearsLastDateStartsAndRedirectsWithSuccessBanner(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeLastDateStartsSubmitModel>> validatorMock,
        [Greedy] ChangeLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        SetupCourseWithLastDateStarts(response, larsCodeServiceMock);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

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

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeLastDateStartsSubmitModel { SelectedOption = ChangeLastDateStartsOptions.Remove },
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

    [Test, MoqAutoData]
    public async Task WhenPosting_AndRouteIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] ChangeLastDateStartsController sut)
    {
        ukprnAndLarsCodeValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IUkprnAndLarsCodeValidator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(IUkprnAndLarsCodeValidator.LarsCode), "invalid")
            ]));

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeLastDateStartsSubmitModel { SelectedOption = ChangeLastDateStartsOptions.Remove },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPosting_AndProviderHasNoLastDateStarts_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] ChangeLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = null }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeLastDateStartsSubmitModel { SelectedOption = ChangeLastDateStartsOptions.Remove },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public void ChangeLastDateStartsOptions_ExposeExpectedValues()
    {
        using (new AssertionScope())
        {
            ChangeLastDateStartsOptions.Change.Should().Be("Change");
            ChangeLastDateStartsOptions.Remove.Should().Be("Remove");
        }
    }

    private static void SetupValidRoute(Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock)
    {
        ukprnAndLarsCodeValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IUkprnAndLarsCodeValidator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private static void SetupCourseWithLastDateStarts(
        GetRestrictedCourseDetailsResponse response,
        Mock<ILarsCodeService> larsCodeServiceMock)
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
}

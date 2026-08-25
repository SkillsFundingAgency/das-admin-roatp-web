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
using SFA.DAS.Admin.Roatp.Web.Validators.Common;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class LastDateStartsControllerSetTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";
    private const int OtherUkprn = 10000001;

    [Test, MoqAutoData]
    public async Task WhenSettingLastDateStarts_AndProviderHasLastDateStartsWithoutChangeJourney_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] LastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        var lastDateStarts = new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Unspecified);
        response.LarsCode = LarsCode;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = lastDateStarts }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        SetupTempData(sut);

        var result = await sut.SetLastDateStarts(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenSettingLastDateStarts_AndProviderHasLastDateStartsAfterChangeJourney_ThenPrepopulatesDateFieldsAndIsChangeMode(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] LastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        var lastDateStarts = new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Unspecified);
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = lastDateStarts }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        SetupChangeExistingDateJourney(sut);

        var result = await sut.SetLastDateStarts(LarsCode, Ukprn, CancellationToken.None) as ViewResult;

        var model = result!.Model as AddLastDateStartsViewModel;
        using (new AssertionScope())
        {
            model!.Day.Should().Be("15");
            model.Month.Should().Be("03");
            model.Year.Should().Be("2027");
            model.IsChangingExistingDate.Should().BeTrue();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastDateStarts_AndProviderExists_ThenReturnsView(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] LastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
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

        var result = await sut.SetLastDateStarts(LarsCode, Ukprn, CancellationToken.None) as ViewResult;

        var model = result!.Model as AddLastDateStartsViewModel;
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(LastDateStartsController.AddLastDateStartsViewPath);
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be("BP TRAINING");
            model.Ukprn.Should().Be(Ukprn);
            model.CourseDisplayTitle.Should().Be("Academic professional (Level 7)");
            model.IsChangingExistingDate.Should().BeFalse();
            model.CancelUrl.Should().Be(RestrictedCourseDetailsUrl);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastDateStarts_AndProviderDoesNotExistOnCourse_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] LastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = OtherUkprn, ProviderName = "Other", LastDateStarts = null }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.SetLastDateStarts(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastDateStarts_AndRouteIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] LastDateStartsController sut)
    {
        ukprnAndLarsCodeValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IUkprnAndLarsCodeValidator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(IUkprnAndLarsCodeValidator.LarsCode), "invalid")
            ]));

        var result = await sut.SetLastDateStarts(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastDateStarts_AndValidationFails_ThenReturnsViewWithErrors(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IValidator<AddLastDateStartsSubmitModel>> validatorMock,
        [Greedy] LastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = null }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<AddLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(AddLastDateStartsSubmitModel.Day), "Enter a valid date")
            ]));

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var submitModel = new AddLastDateStartsSubmitModel
        {
            Day = "",
            Month = "",
            Year = ""
        };

        var result = await sut.SetLastDateStarts(LarsCode, Ukprn, submitModel, CancellationToken.None) as ViewResult;

        var model = result!.Model as AddLastDateStartsViewModel;
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            sut.ModelState.IsValid.Should().BeFalse();
            model!.ProviderName.Should().Be("BP TRAINING");
            model.CourseDisplayTitle.Should().Be("Academic professional (Level 7)");
        }
        validatorMock.Verify(
            v => v.ValidateAsync(
                It.Is<AddLastDateStartsSubmitModel>(m => m.LarsCode == LarsCode),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastDateStarts_AndValidationPasses_ThenCallsApiAndRedirectsWithSuccessBanner(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<AddLastDateStartsSubmitModel>> validatorMock,
        [Greedy] LastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = null }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<AddLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        SetupTestUser(sut);

        var submitModel = new AddLastDateStartsSubmitModel
        {
            Day = "15",
            Month = "03",
            Year = "2027"
        };

        var result = await sut.SetLastDateStarts(LarsCode, Ukprn, submitModel, CancellationToken.None) as RedirectToRouteResult;

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
    public async Task WhenPostingSetLastDateStarts_AndChangingExistingDate_ThenCallsApiAndRedirectsWithUpdatedBanner(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<AddLastDateStartsSubmitModel>> validatorMock,
        [Greedy] LastDateStartsController sut,
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
                LastDateStarts = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified)
            }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<AddLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        SetupTestUser(sut);
        sut.TempData[LastDateStartsController.ChangingExistingLastDateStartsTempDataKey] = true;

        var result = await sut.SetLastDateStarts(
            LarsCode,
            Ukprn,
            new AddLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2026" },
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
    public async Task WhenPostingSetLastDateStarts_AndChangingExistingDateWithoutChangeJourney_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] LastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.Providers =
        [
            new ProviderCourseModel
            {
                Ukprn = Ukprn,
                ProviderName = "BP TRAINING",
                LastDateStarts = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified)
            }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        SetupTempData(sut);

        var result = await sut.SetLastDateStarts(
            LarsCode,
            Ukprn,
            new AddLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2026" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        outerApiClientMock.Verify(
            c => c.UpsertProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastDateStarts_AndCourseDetailsBecomeUnavailable_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] LastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.SetLastDateStarts(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastDateStarts_AndRouteIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] LastDateStartsController sut)
    {
        ukprnAndLarsCodeValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IUkprnAndLarsCodeValidator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(IUkprnAndLarsCodeValidator.LarsCode), "invalid")
            ]));

        var result = await sut.SetLastDateStarts(
            LarsCode,
            Ukprn,
            new AddLastDateStartsSubmitModel { Day = "15", Month = "03", Year = "2027" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastDateStarts_AndProviderDoesNotExistOnCourse_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] LastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = OtherUkprn, ProviderName = "Other", LastDateStarts = null }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.SetLastDateStarts(
            LarsCode,
            Ukprn,
            new AddLastDateStartsSubmitModel { Day = "15", Month = "03", Year = "2027" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupValidRoute(Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock)
    {
        ukprnAndLarsCodeValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IUkprnAndLarsCodeValidator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private static void SetupTempData(Controller sut)
    {
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
    }

    private static void SetupChangeExistingDateJourney(Controller sut)
    {
        SetupTempData(sut);
        sut.TempData[LastDateStartsController.ChangingExistingLastDateStartsTempDataKey] = true;
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

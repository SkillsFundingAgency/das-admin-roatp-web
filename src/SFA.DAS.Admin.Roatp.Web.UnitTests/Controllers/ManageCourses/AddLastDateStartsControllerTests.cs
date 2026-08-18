using System.Security.Claims;
using AutoFixture.NUnit4;
using FluentAssertions;
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
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class AddLastDateStartsControllerTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";
    private const int OtherUkprn = 10000001;

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastDateStarts_AndProviderHasLastDateStarts_ThenPrepopulatesDateFields(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response,
        GetOrganisationResponse organisation)
    {
        var lastDateStarts = new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Unspecified);
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = lastDateStarts }
        ];

        SetupValidUkprn(ukprnServiceMock, organisation);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None) as ViewResult;

        var model = result!.Model as AddLastDateStartsViewModel;
        model!.Day.Should().Be("15");
        model.Month.Should().Be("03");
        model.Year.Should().Be("2027");
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastDateStarts_AndProviderExists_ThenReturnsView(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response,
        GetOrganisationResponse organisation)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = null }
        ];

        SetupValidUkprn(ukprnServiceMock, organisation);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None) as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be(AddLastDateStartsController.ViewPath);
        var model = result.Model as AddLastDateStartsViewModel;
        model.Should().NotBeNull();
        model!.ProviderName.Should().Be("BP TRAINING");
        model.Ukprn.Should().Be(Ukprn);
        model.CourseDisplayTitle.Should().Be("Academic professional (Level 7)");
        model.CancelUrl.Should().Be(RestrictedCourseDetailsUrl);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastDateStarts_AndProviderDoesNotExistOnCourse_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response,
        GetOrganisationResponse organisation)
    {
        response.LarsCode = LarsCode;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = OtherUkprn, ProviderName = "Other", LastDateStarts = null }
        ];

        SetupValidUkprn(ukprnServiceMock, organisation);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastDateStarts_AndUkprnIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        larsCodeServiceMock.Verify(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()), Times.Once);
        ukprnServiceMock.Verify(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastDateStarts_AndValidationFails_ThenReturnsViewWithErrors(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<IValidator<AddLastDateStartsSubmitModel>> validatorMock,
        [Greedy] AddLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response,
        GetOrganisationResponse organisation)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = null }
        ];

        SetupValidUkprn(ukprnServiceMock, organisation);
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

        var result = await sut.Index(LarsCode, Ukprn, submitModel, CancellationToken.None) as ViewResult;

        result.Should().NotBeNull();
        sut.ModelState.IsValid.Should().BeFalse();
        var model = result!.Model as AddLastDateStartsViewModel;
        model!.ProviderName.Should().Be("BP TRAINING");
        model.CourseDisplayTitle.Should().Be("Academic professional (Level 7)");
        validatorMock.Verify(
            v => v.ValidateAsync(
                It.Is<AddLastDateStartsSubmitModel>(m => m.LarsCode == LarsCode),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastDateStarts_AndValidationPasses_ThenCallsApiAndRedirectsWithSuccessBanner(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<AddLastDateStartsSubmitModel>> validatorMock,
        [Greedy] AddLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response,
        GetOrganisationResponse organisation)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = null }
        ];

        SetupValidUkprn(ukprnServiceMock, organisation);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<AddLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Jane"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "Denver"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn", "jane@education.gov.uk")
                ], "test"))
            }
        };
        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

        var submitModel = new AddLastDateStartsSubmitModel
        {
            Day = "15",
            Month = "03",
            Year = "2027"
        };

        var result = await sut.Index(LarsCode, Ukprn, submitModel, CancellationToken.None) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.RestrictedCourseDetails);
        result.RouteValues!["larsCode"].Should().Be(LarsCode);
        sut.TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey]
            .Should().Be("Last start date added for BP TRAINING");

        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            Ukprn,
            LarsCode,
            It.Is<UpsertProviderAllowedCourseRequest>(r =>
                r.LastDateStarts == new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Unspecified)
                && r.UserId == "jane@education.gov.uk"
                && r.UserDisplayName == "Jane Denver"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastDateStarts_AndLarsCodeIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] AddLastDateStartsController sut)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        larsCodeServiceMock.Verify(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddLastDateStarts_AndCourseDetailsBecomeUnavailable_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response,
        GetOrganisationResponse organisation)
    {
        response.LarsCode = LarsCode;

        SetupValidUkprn(ukprnServiceMock, organisation);
        larsCodeServiceMock
            .SetupSequence(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response)
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastDateStarts_AndProviderDoesNotExist_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new AddLastDateStartsSubmitModel { Day = "15", Month = "03", Year = "2027" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastDateStarts_AndProviderDoesNotExistOnCourse_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] AddLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response,
        GetOrganisationResponse organisation)
    {
        response.LarsCode = LarsCode;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = OtherUkprn, ProviderName = "Other", LastDateStarts = null }
        ];

        SetupValidUkprn(ukprnServiceMock, organisation);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new AddLastDateStartsSubmitModel { Day = "15", Month = "03", Year = "2027" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupValidUkprn(
        Mock<IUkprnService> ukprnServiceMock,
        GetOrganisationResponse organisation)
    {
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
    }
}

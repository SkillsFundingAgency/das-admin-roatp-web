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
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Frozen] Mock<IValidator<ChangeCourseRestrictionSubmitModel>> validatorMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidRoute(restrictedCourseProviderServiceMock);
        SetupCourseWithLastDateStarts(restrictedCourseProviderServiceMock, response);
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
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeCourseRestrictionSubmitModel>> validatorMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidRoute(restrictedCourseProviderServiceMock);
        SetupCourseWithLastDateStarts(restrictedCourseProviderServiceMock, response);
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
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeCourseRestrictionSubmitModel>> validatorMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidRoute(restrictedCourseProviderServiceMock);
        SetupCourseWithLastDateStarts(restrictedCourseProviderServiceMock, response);
        SetupUpsertRequest(restrictedCourseProviderServiceMock);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeCourseRestrictionSubmitModel>(), It.IsAny<CancellationToken>()))
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

    [Test, MoqAutoData]
    public async Task WhenRouteIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] ChangeCourseRestrictionController sut)
    {
        restrictedCourseProviderServiceMock
            .Setup(s => s.IsRouteValidAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeCourseRestrictionSubmitModel { SelectedOption = ChangeCourseRestrictionOptions.Remove },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        restrictedCourseProviderServiceMock.Verify(
            s => s.GetCourseAndProviderAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenProviderHasNoLastDateStarts_ThenReturnsNotFound(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        var provider = new ProviderCourseModel
        {
            Ukprn = Ukprn,
            ProviderName = "BP TRAINING",
            LastDateStarts = null
        };

        SetupValidRoute(restrictedCourseProviderServiceMock);
        restrictedCourseProviderServiceMock
            .Setup(s => s.GetCourseAndProviderAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((response, provider));

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(
            LarsCode,
            Ukprn,
            new ChangeCourseRestrictionSubmitModel { SelectedOption = ChangeCourseRestrictionOptions.Remove },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupValidRoute(Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock)
    {
        restrictedCourseProviderServiceMock
            .Setup(s => s.IsRouteValidAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private static void SetupCourseWithLastDateStarts(
        Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        var provider = new ProviderCourseModel
        {
            Ukprn = Ukprn,
            ProviderName = "BP TRAINING",
            LastDateStarts = LastDateStarts
        };

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
}

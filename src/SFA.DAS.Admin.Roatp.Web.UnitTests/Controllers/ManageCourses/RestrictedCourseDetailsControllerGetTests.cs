using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class RestrictedCourseDetailsControllerGetTests
{
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndLarsCodeIsValid_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourseWithProviders(response, outerApiClientMock);
        SetupUrlHelper(sut);

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RestrictedCourseDetailsController.ViewPath);
            var model = result.Model as RestrictedCourseDetailsViewModel;
            model.Should().NotBeNull();
            model!.DisplayTitle.Should().Be("Academic professional (Level 7)");
            model.Sector.Should().Be("Education and early years");
            model.LarsCode.Should().Be(LarsCode);
            model.Level.Should().Be(7);
            model.StatusText.Should().Be("Restricted");
            model.HasActiveFilters.Should().BeFalse();
            model.Filters.ShowFilterOptions.Should().BeFalse();
            model.Filters.FilterSections.Should().HaveCount(2);
        }

        outerApiClientMock.Verify(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndProvidersExist_ThenMapsAllowedProvidersSortedByName(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourseWithProviders(response, outerApiClientMock);
        SetupUrlHelper(sut);

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedCourseDetailsViewModel;

        using (new AssertionScope())
        {
            model!.HasProviders.Should().BeTrue();
            model.ProviderCount.Should().Be(2);
            model.ProviderCountDescription.Should().Be("2 providers");
            model.AllowedProviders.Select(p => p.ProviderName).Should().ContainInOrder("ACORN SKILLS TRAINING", "BABINGTON LTD");
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndProviderHasNoLastDateStarts_ThenDeliveryStatusIsOpenToNewStarts(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourseWithProviders(response, outerApiClientMock);
        SetupUrlHelper(sut);

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedCourseDetailsViewModel;
        var provider = model!.AllowedProviders.First(p => !p.HasLastDateStarts);

        using (new AssertionScope())
        {
            provider.DeliveryStatus.Should().Be(DeliveryStatus.OpenToNewStarts);
            provider.ChangeUrl.Should().Be("/set-last-date-starts");
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndProviderHasLastDateStarts_ThenDeliveryStatusIsLastStartDateAdded(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourseWithProviders(response, outerApiClientMock);
        SetupUrlHelper(sut);

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedCourseDetailsViewModel;
        var provider = model!.AllowedProviders.First(p => p.HasLastDateStarts);

        using (new AssertionScope())
        {
            provider.DeliveryStatus.Should().Be(DeliveryStatus.LastStartDateAdded);
            provider.ChangeUrl.Should().Be("/change-restriction");
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndNoProviders_ThenReturnsEmptyState(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;
        response.Providers = [];

        SetupCourseResponse(outerApiClientMock, response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, "/restricted-courses")
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        using (new AssertionScope())
        {
            model!.HasNoProviders.Should().BeTrue();
            model.HasNoFilterResults.Should().BeFalse();
            model.AllowedProviders.Should().BeEmpty();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndTempDataContainsSuccessBannerMessage_ThenModelHasSuccessBannerMessage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response,
        string successMessage)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;
        response.Providers = [];

        SetupCourseResponse(outerApiClientMock, response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, "/restricted-courses")
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        SetupTempData(sut);
        sut.TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey] = successMessage;

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.SuccessBannerMessage.Should().Be(successMessage);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndTempDataContainsRestrictCourseSuccessBanner_ThenModelHasSuccessBannerMessage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;
        response.Providers = [];

        SetupCourseResponse(outerApiClientMock, response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, "/restricted-courses")
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        SetupTempData(sut);
        sut.TempData[RestrictedCourseDetailsController.SuccessBannerTempDataKey] =
            RestrictCourseConfirmController.SuccessBannerMessage;

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        using (new AssertionScope())
        {
            model!.SuccessBannerMessage.Should().Be(RestrictCourseConfirmController.SuccessBannerMessage);
            model.HasSuccessBanner.Should().BeTrue();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndLarsCodeIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound), null, new RefitSettings(), null));

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        outerApiClientMock.Verify(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase(HttpStatusCode.BadRequest)]
    public async Task WhenGettingRestrictedCourseDetails_AndCourseApiReturnsBadRequest_ThenReturnsNotFound(
        HttpStatusCode statusCode)
    {
        var outerApiClientMock = new Mock<IOuterApiClient>();
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(statusCode), null, new RefitSettings(), null));

        var sut = new RestrictedCourseDetailsController(outerApiClientMock.Object);

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndCourseApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.InternalServerError), null, new RefitSettings(), null));

        var act = () => sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage($"Failed to retrieve course details for LARS code '{LarsCode}'. Status code: InternalServerError.");
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndCourseIsUnrestricted_ThenRedirectsToUnrestrictedDetails(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = false;

        SetupCourseResponse(outerApiClientMock, response);

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.UnrestrictedCourseDetails);
            result.RouteValues!["larsCode"].Should().Be(LarsCode);
        }
    }

    private static void SetupRestrictedCourseWithProviders(
        GetRestrictedCourseDetailsResponse response,
        Mock<IOuterApiClient> outerApiClientMock)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Route = "Education and early years";
        response.LearningType = LearningType.Apprenticeship;
        response.IsCourseRestricted = true;
        response.Providers =
        [
            new ProviderCourseModel
            {
                Ukprn = 10019900,
                ProviderName = "BABINGTON LTD",
                LastDateStarts = DateTime.UtcNow.Date.AddDays(30)
            },
            new ProviderCourseModel
            {
                Ukprn = 10000001,
                ProviderName = "ACORN SKILLS TRAINING",
                LastDateStarts = null
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

    private static void SetupUrlHelper(RestrictedCourseDetailsController sut)
    {
        SetupTempData(sut);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, "/restricted-courses")
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl)
            .AddUrlForRoute(RouteNames.SetLastDateStarts, "/set-last-date-starts")
            .AddUrlForRoute(RouteNames.ChangeCourseRestriction, "/change-restriction");
    }

    private static void SetupTempData(RestrictedCourseDetailsController sut)
    {
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
    }
}

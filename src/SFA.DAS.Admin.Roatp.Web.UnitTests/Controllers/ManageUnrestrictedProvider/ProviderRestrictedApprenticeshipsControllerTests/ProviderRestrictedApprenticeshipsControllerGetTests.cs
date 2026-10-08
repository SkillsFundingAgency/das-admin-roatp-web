using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ProviderRestrictedApprenticeshipsControllerTests;

[TestFixture]
public class ProviderRestrictedApprenticeshipsControllerGetTests
{
    private const string ProviderSummaryUrl = "/providers/10019900";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";
    private const string RestrictCourseSearchUrl = "/providers/10019900/restricted-courses/add";
    private const string ProviderRestrictedCourseChangeRestrictionUrl = "/providers/10019900/restricted-courses/105/change-restriction";

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndProviderNameIsInSession_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderRestrictedApprenticeshipsController sut,
        GetRestrictedApprenticeshipsResponse response,
        string providerName,
        int ukprn)
    {
        response.Courses =
        [
            new ProviderRestrictedApprenticeshipModel
            {
                LarsCode = "200",
                Title = "Zebra course",
                Level = 3,
                LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
                IsClosedToNewStarts = true
            },
            new ProviderRestrictedApprenticeshipModel
            {
                LarsCode = "105",
                Title = "Alpha course",
                Level = 6,
                LastDateStarts = DateTime.UtcNow.Date.AddDays(5),
                IsClosedToNewStarts = false
            }
        ];

        sut.AddTempData();
        sessionServiceMock.SetupProviderName(ukprn, providerName);
        ukprnServiceMock.SetupOrganisationRestriction(ukprn, isRestricted: false);
        SetupRestrictedApprenticeships(outerApiClientMock, ukprn, response);
        SetupUrlHelper(sut);

        var result = await sut.Index(ukprn, new GetProviderRestrictedApprenticeshipsRequestModel(), CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ProviderRestrictedApprenticeshipsController.ViewPath);
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be(providerName);
            model.BackLinkUrl.Should().Be(ProviderSummaryUrl);
            model.BackLinkText.Should().Be(RestrictedApprenticeshipsViewModel.BackLinkTextValue);
            model.RestrictACourseUrl.Should().Be(RestrictCourseSearchUrl);
            model.HasCourses.Should().BeTrue();
            model.HasActiveFilters.Should().BeFalse();
            model.ShowCourseResults.Should().BeTrue();
            model.Filters.FilterSections.Should().HaveCount(2);
            model.Courses.Select(course => course.LarsCode).Should().Equal("105", "200");
            model.Courses.Select(course => course.DeliveryStatus).Should().Equal(
                DeliveryStatus.LastStartDateAdded,
                DeliveryStatus.ClosedToNewStarts);
            model.Courses.Should().OnlyContain(course => course.ChangeUrl == ProviderRestrictedCourseChangeRestrictionUrl);
            model.HasSuccessBanner.Should().BeFalse();
        }

        outerApiClientMock.Verify(c => c.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndProviderIsRestricted_ThenRedirectsToAllowedCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderRestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        sut.AddTempData();
        sessionServiceMock.SetupProviderName(ukprn, providerName);
        ukprnServiceMock.SetupOrganisationRestriction(ukprn, isRestricted: true);

        var result = await sut.Index(ukprn, new GetProviderRestrictedApprenticeshipsRequestModel(), CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderAllowedCourses);
            result.RouteValues!["ukprn"].Should().Be(ukprn);
            result.RouteValues["courseType"].Should().Be(CourseType.Apprenticeship);
        }

        outerApiClientMock.Verify(
            c => c.GetRestrictedApprenticeships(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndTempDataContainsSuccessBanner_ThenModelHasSuccessBanner(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderRestrictedApprenticeshipsController sut,
        GetRestrictedApprenticeshipsResponse response,
        string providerName,
        int ukprn)
    {
        const string successMessage = "Carpentry (Level 1) has been added to the restricted apprenticeships list";
        response.Courses = [];

        sut.AddTempData();
        sessionServiceMock.SetupProviderName(ukprn, providerName);
        ukprnServiceMock.SetupOrganisationRestriction(ukprn, isRestricted: false);
        sut.TempData[ProviderRestrictedApprenticeshipsController.SuccessBannerTempDataKey] = successMessage;
        SetupRestrictedApprenticeships(outerApiClientMock, ukprn, response);
        SetupUrlHelper(sut);

        var result = await sut.Index(ukprn, new GetProviderRestrictedApprenticeshipsRequestModel(), CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            model.Should().NotBeNull();
            model!.SuccessBannerMessage.Should().Be(successMessage);
            model.HasSuccessBanner.Should().BeTrue();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndProviderNameIsNotInSession_ThenLoadsNameFromOrganisation(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderRestrictedApprenticeshipsController sut,
        GetOrganisationResponse organisationResponse,
        GetRestrictedApprenticeshipsResponse restrictedResponse,
        int ukprn)
    {
        organisationResponse.Ukprn = ukprn;
        restrictedResponse.Courses = [];

        sut.AddTempData();
        sessionServiceMock.SetupProviderNameMissing();
        ukprnServiceMock.SetupOrganisationRestriction(ukprn, isRestricted: false);
        SetupOrganisation(outerApiClientMock, ukprn, organisationResponse, HttpStatusCode.OK);
        SetupRestrictedApprenticeships(outerApiClientMock, ukprn, restrictedResponse);
        SetupUrlHelper(sut);

        var result = await sut.Index(ukprn, new GetProviderRestrictedApprenticeshipsRequestModel(), CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be(organisationResponse.LegalName);
            model.HasNoCourses.Should().BeTrue();
            model.ShowCourseResults.Should().BeFalse();
        }

        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.ProviderNames,
            It.Is<Dictionary<int, string>>(d => d[ukprn] == organisationResponse.LegalName)), Times.Once);
        outerApiClientMock.Verify(c => c.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndUkprnServiceReturnsNoOrganisation_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderRestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        sut.AddTempData();
        sessionServiceMock.SetupProviderName(ukprn, providerName);
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);

        var result = await sut.Index(ukprn, new GetProviderRestrictedApprenticeshipsRequestModel(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().BeOfType<NotFoundResult>();
            outerApiClientMock.Verify(
                c => c.GetRestrictedApprenticeships(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndOrganisationIsNotFound_ThenRedirectsToNotFoundPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderRestrictedApprenticeshipsController sut,
        GetOrganisationResponse organisationResponse,
        int ukprn)
    {
        sut.AddTempData();
        sessionServiceMock.SetupProviderNameMissing();
        SetupOrganisation(outerApiClientMock, ukprn, organisationResponse, HttpStatusCode.NotFound);

        var result = await sut.Index(ukprn, new GetProviderRestrictedApprenticeshipsRequestModel(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();
        }

        outerApiClientMock.Verify(
            c => c.GetRestrictedApprenticeships(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndRestrictedApprenticeshipsAreNotFound_ThenRedirectsToNotFoundPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderRestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        sut.AddTempData();
        sessionServiceMock.SetupProviderName(ukprn, providerName);
        ukprnServiceMock.SetupOrganisationRestriction(ukprn, isRestricted: false);
        SetupRestrictedApprenticeships(outerApiClientMock, ukprn, statusCode: HttpStatusCode.NotFound);

        var result = await sut.Index(ukprn, new GetProviderRestrictedApprenticeshipsRequestModel(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderRestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        sut.AddTempData();
        sessionServiceMock.SetupProviderName(ukprn, providerName);
        ukprnServiceMock.SetupOrganisationRestriction(ukprn, isRestricted: false);
        SetupRestrictedApprenticeships(outerApiClientMock, ukprn, statusCode: HttpStatusCode.InternalServerError);

        var act = () => sut.Index(ukprn, new GetProviderRestrictedApprenticeshipsRequestModel(), CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();

        outerApiClientMock.Verify(
            c => c.GetRestrictedApprenticeships(ukprn, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndResponseContentIsNull_ThenReturnsViewWithNoCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderRestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        sut.AddTempData();
        sessionServiceMock.SetupProviderName(ukprn, providerName);
        ukprnServiceMock.SetupOrganisationRestriction(ukprn, isRestricted: false);
        SetupRestrictedApprenticeships(outerApiClientMock, ukprn, response: null);
        SetupUrlHelper(sut);

        var result = await sut.Index(ukprn, new GetProviderRestrictedApprenticeshipsRequestModel(), CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ProviderRestrictedApprenticeshipsController.ViewPath);
            model.Should().NotBeNull();
            model!.HasNoCourses.Should().BeTrue();
            model.ShowCourseResults.Should().BeFalse();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndCoursesAreNull_ThenReturnsViewWithNoCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderRestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        sut.AddTempData();
        sessionServiceMock.SetupProviderName(ukprn, providerName);
        ukprnServiceMock.SetupOrganisationRestriction(ukprn, isRestricted: false);
        SetupRestrictedApprenticeships(
            outerApiClientMock,
            ukprn,
            new GetRestrictedApprenticeshipsResponse { Courses = null! });
        SetupUrlHelper(sut);

        var result = await sut.Index(ukprn, new GetProviderRestrictedApprenticeshipsRequestModel(), CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ProviderRestrictedApprenticeshipsController.ViewPath);
            model.Should().NotBeNull();
            model!.HasNoCourses.Should().BeTrue();
            model.ShowCourseResults.Should().BeFalse();
        }
    }

    private static void SetupUrlHelper(ProviderRestrictedApprenticeshipsController sut)
    {
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderSummary, ProviderSummaryUrl)
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl)
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourseSearch, RestrictCourseSearchUrl)
            .AddUrlForRoute(RouteNames.RestrictedCourseChangeRestriction, ProviderRestrictedCourseChangeRestrictionUrl);
    }

    private static void SetupRestrictedApprenticeships(
        Mock<IOuterApiClient> outerApiClientMock,
        int ukprn,
        GetRestrictedApprenticeshipsResponse? response = null,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        ApiException? apiException = null;
        if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NotFound)
        {
            apiException = ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Get,
                httpResponse,
                new RefitSettings()).GetAwaiter().GetResult();
        }

        outerApiClientMock
            .Setup(c => c.GetRestrictedApprenticeships(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedApprenticeshipsResponse>(
                httpResponse,
                response,
                new RefitSettings(),
                apiException));
    }

    private static void SetupOrganisation(
        Mock<IOuterApiClient> outerApiClientMock,
        int ukprn,
        GetOrganisationResponse response,
        HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(statusCode),
                response,
                new RefitSettings(),
                null));
    }
}

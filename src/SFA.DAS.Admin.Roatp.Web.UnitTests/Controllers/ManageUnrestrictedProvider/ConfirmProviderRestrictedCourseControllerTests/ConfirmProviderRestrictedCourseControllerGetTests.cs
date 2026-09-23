using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ConfirmProviderRestrictedCourseControllerTests;

public class ConfirmProviderRestrictedCourseControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string DisplayTitle = "Carpentry (Level 1)";
    private const string ProviderName = "Denton Business Services Limited";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";

    [Test, MoqAutoData]
    public async Task WhenGettingConfirm_AndSessionExists_ThenReturnsViewWithCourseDetails(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut)
    {
        SetupSession(sessionServiceMock);
        sut.AddTempData();
        sut.TempData[TempDataKeys.ProviderLegalName] = ProviderName;
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(Ukprn) as ViewResult;
        var model = result?.Model as ConfirmProviderRestrictedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ConfirmProviderRestrictedCourseController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.ProviderName.Should().Be(ProviderName);
            model.DisplayTitle.Should().Be(DisplayTitle);
            model.LarsCode.Should().Be(LarsCode);
            model.CancelUrl.Should().Be(RestrictedCoursesUrl);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingConfirm_AndProviderNameIsNotInTempData_ThenLoadsNameFromOrganisation(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut,
        GetOrganisationResponse organisationResponse)
    {
        organisationResponse.Ukprn = Ukprn;
        SetupSession(sessionServiceMock);
        sut.AddTempData();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);
        SetupOrganisation(outerApiClientMock, organisationResponse, HttpStatusCode.OK);

        var result = await sut.Index(Ukprn) as ViewResult;
        var model = result?.Model as ConfirmProviderRestrictedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be(organisationResponse.LegalName);
            sut.TempData.Peek(TempDataKeys.ProviderLegalName).Should().Be(organisationResponse.LegalName);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingConfirm_AndSessionIsMissing_ThenRedirectsToRestrictedApprenticeshipsList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<RestrictCourseSessionModel>(SessionKeys.RestrictCourse))
            .Returns((RestrictCourseSessionModel?)null);

        var result = await sut.Index(Ukprn) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingConfirm_AndSessionUkprnDoesNotMatch_ThenRedirectsToRestrictedApprenticeshipsList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut)
    {
        SetupSession(sessionServiceMock, ukprn: 99999999);

        var result = await sut.Index(Ukprn) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingConfirm_AndOrganisationIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmProviderRestrictedCourseController sut,
        GetOrganisationResponse organisationResponse)
    {
        SetupSession(sessionServiceMock);
        sut.AddTempData();
        SetupOrganisation(outerApiClientMock, organisationResponse, HttpStatusCode.NotFound);

        var result = await sut.Index(Ukprn);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupSession(Mock<ISessionService> sessionServiceMock, int ukprn = Ukprn)
    {
        sessionServiceMock
            .Setup(s => s.Get<RestrictCourseSessionModel>(SessionKeys.RestrictCourse))
            .Returns(new RestrictCourseSessionModel
            {
                Ukprn = ukprn,
                LarsCode = LarsCode,
                Title = "Carpentry",
                Level = 1,
                DisplayTitle = DisplayTitle
            });
    }

    private static void SetupOrganisation(
        Mock<IOuterApiClient> outerApiClientMock,
        GetOrganisationResponse response,
        HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(statusCode),
                response,
                new RefitSettings(),
                null));
    }
}

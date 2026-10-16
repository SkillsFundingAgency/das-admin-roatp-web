using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ProviderSummaryControllerTests;

public class ProviderSummaryControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string SelectOrganisationUrl = "/providers";
    private const string ProviderStatusUpdateUrl = "/providers/10019900/status";
    private const string ProviderTypeUpdateUrl = "/providers/10019900/provider-type";
    private const string OrganisationTypeUpdateUrl = "/providers/10019900/organisation-type";
    private const string ApprenticeshipUnitsUpdateUrl = "/providers/10019900/apprenticeship-units";
    private const string RestrictProviderUrl = "/providers/10019900/restrict";
    private const string ProviderRestrictedCoursesUrl = "/providers/10019900/restricted-courses";
    private const string ProviderAllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";
    private const string ProviderAllowedUnitsUrl = "/providers/10019900/ApprenticeshipUnit/allowed-courses";

    [Test, MoqAutoData]
    public async Task WhenGettingProviderSummary_AndNoMatchingDetails_ThenRedirectsToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] EditOrganisationSessionModel _editOrganisationSessionModel,
        [Greedy] ProviderSummaryController sut,
        int ukprn,
        CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingProviderSummary_AndMatchingDetails_ThenSetsModelUrls(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ProviderSummaryController sut,
        GetOrganisationResponse getOrganisationResponse)
    {
        getOrganisationResponse.Ukprn = Ukprn;
        sessionServiceMock.SetupProviderNameMissing();
        var urlHelperMock = sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.SelectProvider, SelectOrganisationUrl)
            .AddUrlForRoute(RouteNames.ProviderStatusUpdate, ProviderStatusUpdateUrl)
            .AddUrlForRoute(RouteNames.ProviderTypeUpdate, ProviderTypeUpdateUrl)
            .AddUrlForRoute(RouteNames.OrganisationTypeUpdate, OrganisationTypeUpdateUrl)
            .AddUrlForRoute(RouteNames.ApprenticeshipUnitsUpdate, ApprenticeshipUnitsUpdateUrl)
            .AddUrlForRoute(RouteNames.RestrictProvider, RestrictProviderUrl)
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, ProviderRestrictedCoursesUrl);

        urlHelperMock
            .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c =>
                HasRouteLearningType(c, RouteLearningType.Apprenticeship))))
            .Returns(ProviderAllowedCoursesUrl);
        urlHelperMock
            .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c =>
                HasRouteLearningType(c, RouteLearningType.ApprenticeshipUnit))))
            .Returns(ProviderAllowedUnitsUrl);

        outerApiClientMock.Setup(x => x.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(Ukprn, CancellationToken.None) as ViewResult;
        var model = actual?.Model as ProviderSummaryViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.SearchProviderUrl.Should().Be(SelectOrganisationUrl);
            model.StatusChangeLink.Should().Be(ProviderStatusUpdateUrl);
            model.ProviderTypeChangeLink.Should().Be(ProviderTypeUpdateUrl);
            model.OrganisationTypeChangeLink.Should().Be(OrganisationTypeUpdateUrl);
            model.OffersApprenticeshipUnitsChangeLink.Should().Be(ApprenticeshipUnitsUpdateUrl);
            model.ManageRestrictedCoursesUrl.Should().Be(ProviderRestrictedCoursesUrl);
            model.ManageApprovedCoursesUrl.Should().Be(ProviderAllowedCoursesUrl);
            model.ChangeHowWeManageThisProviderUrl.Should().Be(RestrictProviderUrl);
            model.ManageApprovedUnitsUrl.Should().Be(ProviderAllowedUnitsUrl);
        }

        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.ProviderNames,
            It.Is<Dictionary<int, string>>(d => d[Ukprn] == getOrganisationResponse.LegalName)), Times.Once);
    }

    private static bool HasRouteLearningType(UrlRouteContext context, RouteLearningType routeLearningType)
        => context.RouteName!.Equals(RouteNames.ProviderAllowedCourses)
           && Equals(
               context.Values?.GetType().GetProperty("routeLearningType")?.GetValue(context.Values),
               routeLearningType);
}

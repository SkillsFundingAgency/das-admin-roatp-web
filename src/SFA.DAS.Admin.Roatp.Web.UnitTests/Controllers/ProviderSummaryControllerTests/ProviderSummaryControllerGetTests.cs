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
        [Frozen] EditOrganisationSessionModel _editOrganisationSessionModel,
        [Greedy] ProviderSummaryController sut,
        string selectOrganisationLink,
        string providerStatusUpdateLink,
        string providerTypeUpdateLink,
        string organisationTypeUpdateLink,
        string apprenticeshipUnitsUpdateLink,
        string providerSummaryLink,
        string providerRestrictedCoursesLink,
        string providerAllowedCoursesLink,
        string providerAllowedUnitsLink,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.Ukprn = ukprn;
        _editOrganisationSessionModel.Ukprn = ukprn;
        sessionServiceMock.SetupProviderNameMissing();
        var urlHelperMock = sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.SelectProvider, selectOrganisationLink)
            .AddUrlForRoute(RouteNames.ProviderStatusUpdate, providerStatusUpdateLink)
            .AddUrlForRoute(RouteNames.ProviderTypeUpdate, providerTypeUpdateLink)
            .AddUrlForRoute(RouteNames.OrganisationTypeUpdate, organisationTypeUpdateLink)
            .AddUrlForRoute(RouteNames.ApprenticeshipUnitsUpdate, apprenticeshipUnitsUpdateLink)
            .AddUrlForRoute(RouteNames.ProviderSummary, providerSummaryLink)
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, providerRestrictedCoursesLink);

        urlHelperMock
            .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c =>
                HasRouteLearningType(c, RouteLearningType.Apprenticeship))))
            .Returns(providerAllowedCoursesLink);
        urlHelperMock
            .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c =>
                HasRouteLearningType(c, RouteLearningType.ApprenticeshipUnit))))
            .Returns(providerAllowedUnitsLink);

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(_editOrganisationSessionModel.Ukprn, cancellationToken) as ViewResult;
        var model = actual?.Model as ProviderSummaryViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(ukprn);
            model.SearchProviderUrl.Should().Be(selectOrganisationLink);
            model.StatusChangeLink.Should().Be(providerStatusUpdateLink);
            model.ProviderTypeChangeLink.Should().Be(providerTypeUpdateLink);
            model.OrganisationTypeChangeLink.Should().Be(organisationTypeUpdateLink);
            model.OffersApprenticeshipUnitsChangeLink.Should().Be(apprenticeshipUnitsUpdateLink);
            model.ManageRestrictedCoursesUrl.Should().Be(providerRestrictedCoursesLink);
            model.ManageApprovedCoursesUrl.Should().Be(providerAllowedCoursesLink);
            model.ChangeHowWeManageThisProviderUrl.Should().Be(providerSummaryLink);
            model.ManageApprovedUnitsUrl.Should().Be(providerAllowedUnitsLink);
        }

        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.ProviderNames,
            It.Is<Dictionary<int, string>>(d => d[ukprn] == getOrganisationResponse.LegalName)), Times.Once);
    }

    private static bool HasRouteLearningType(UrlRouteContext context, RouteLearningType routeLearningType)
        => context.RouteName!.Equals(RouteNames.ProviderAllowedCourses)
           && Equals(
               context.Values?.GetType().GetProperty("routeLearningType")?.GetValue(context.Values),
               routeLearningType);
}

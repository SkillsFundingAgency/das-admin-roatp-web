using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ProviderSummaryControllerTests;
public class GetProviderSummaryControllerTests
{
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] EditOrganisationSessionModel _editOrganisationSessionModel,
        [Greedy] ProviderSummaryController sut,
        int ukprn,
        CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((ApiResponse<GetOrganisationResponse>)null!);

        var actual = await sut.Index(ukprn, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_SetSessionAndRedirect(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] EditOrganisationSessionModel _editOrganisationSessionModel,
        [Greedy] ProviderSummaryController sut,
        string selectOrganisationLink,
        string providerStatusUpdateLink,
        string providerTypeUpdateLink,
        string organisationTypeUpdateLink,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.Ukprn = ukprn;
        _editOrganisationSessionModel.Ukprn = ukprn;
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.SelectProvider, selectOrganisationLink)
            .AddUrlForRoute(RouteNames.ProviderStatusUpdate, providerStatusUpdateLink)
            .AddUrlForRoute(RouteNames.ProviderTypeUpdate, providerTypeUpdateLink)
            .AddUrlForRoute(RouteNames.OrganisationTypeUpdate, organisationTypeUpdateLink)
            ;

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(_editOrganisationSessionModel.Ukprn, cancellationToken) as ViewResult;
        actual.Should().NotBeNull();
        var model = actual.Model as ProviderSummaryViewModel;
        model.Should().NotBeNull();
        model.Ukprn.Should().Be(ukprn);
        model.SearchProviderUrl.Should().Be(selectOrganisationLink);
        model.StatusChangeLink.Should().Be(providerStatusUpdateLink);
        model.ProviderTypeChangeLink.Should().Be(providerTypeUpdateLink);
        model.OrganisationTypeChangeLink.Should().Be(organisationTypeUpdateLink);
    }
}

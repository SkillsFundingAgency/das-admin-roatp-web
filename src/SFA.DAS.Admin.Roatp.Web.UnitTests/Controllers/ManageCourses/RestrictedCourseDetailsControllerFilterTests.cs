using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class RestrictedCourseDetailsControllerFilterTests
{
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";
    private const string AddProviderToRestrictedCourseUrl = "/add-provider";

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndProviderNameFilterMatches_ThenReturnsMatchingProviders(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = 10019900, ProviderName = "BABINGTON LTD", LastDateStarts = null },
            new ProviderCourseModel { Ukprn = 10000001, ProviderName = "BEACON APPRENTICESHIP SERVICES", LastDateStarts = null }
        ];

        SetupCourseResponse(outerApiClientMock, response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl)
            .AddUrlForRoute(RouteNames.AddProviderToRestrictedCourse, AddProviderToRestrictedCourseUrl);

        var request = new GetRestrictedCourseDetailsRequest { SearchTerm = "Beacon" };

        var result = await sut.Index(LarsCode, request, CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.HasActiveFilters.Should().BeTrue();
        model.HasProviders.Should().BeTrue();
        model.AllowedProviders.Should().ContainSingle(p => p.ProviderName == "BEACON APPRENTICESHIP SERVICES");
        model.Filters.ShowFilterOptions.Should().BeTrue();
        model.Filters.ClearFilterSections.Should().ContainSingle(section => section.Title == "Provider name");
        model.Filters.ClearFilterSections.Single().Items.Single().DisplayText.Should().Be("Beacon");
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndDeliveryStatusFilterMatches_ThenReturnsMatchingProviders(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = 1, ProviderName = "Open Provider", LastDateStarts = null },
            new ProviderCourseModel { Ukprn = 2, ProviderName = "Last Start Provider", LastDateStarts = DateTime.UtcNow.Date.AddDays(10) },
            new ProviderCourseModel { Ukprn = 3, ProviderName = "Closed Provider", LastDateStarts = DateTime.UtcNow.Date.AddDays(-1) }
        ];

        SetupCourseResponse(outerApiClientMock, response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl)
            .AddUrlForRoute(RouteNames.AddProviderToRestrictedCourse, AddProviderToRestrictedCourseUrl);

        var request = new GetRestrictedCourseDetailsRequest
        {
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded]
        };

        var result = await sut.Index(LarsCode, request, CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.AllowedProviders.Should().ContainSingle(p => p.ProviderName == "Last Start Provider");
        model.Filters.ClearFilterSections.Should().ContainSingle(section => section.Title == "Delivery status");
        model.Filters.FilterSections.Should().Contain(section =>
            section.For == nameof(FilterType.DeliveryStatus));
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndFiltersHaveNoMatches_ThenShowsNoFilterResults(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = 10019900, ProviderName = "BABINGTON LTD", LastDateStarts = null }
        ];

        SetupCourseResponse(outerApiClientMock, response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl)
            .AddUrlForRoute(RouteNames.AddProviderToRestrictedCourse, AddProviderToRestrictedCourseUrl);

        var request = new GetRestrictedCourseDetailsRequest { SearchTerm = "Beacon" };

        var result = await sut.Index(LarsCode, request, CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.HasNoFilterResults.Should().BeTrue();
        model.HasNoProviders.Should().BeFalse();
        model.HasActiveFilters.Should().BeTrue();
        model.AllowedProviders.Should().BeEmpty();
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
}

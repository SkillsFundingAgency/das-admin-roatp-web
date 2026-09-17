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
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider;

[TestFixture]
public class RestrictedApprenticeshipsControllerFilterTests
{
    private const string ProviderSummaryUrl = "/providers/10019900";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndCourseNameFilterMatches_ThenReturnsMatchingCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        GetRestrictedApprenticeshipsResponse response,
        string providerName,
        int ukprn)
    {
        response.Courses =
        [
            new RestrictedApprenticeshipModel
            {
                LarsCode = "200",
                Title = "Zebra course",
                Level = 3,
                LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
                IsClosedToNewStarts = true
            },
            new RestrictedApprenticeshipModel
            {
                LarsCode = "105",
                Title = "Alpha course",
                Level = 6,
                LastDateStarts = DateTime.UtcNow.Date.AddDays(5),
                IsClosedToNewStarts = false
            }
        ];

        SetupController(sut, outerApiClientMock, ukprn, providerName, response);

        var result = await sut.Index(
            ukprn,
            new GetRestrictedApprenticeshipsRequest { SearchTerm = "Alpha" },
            CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            model!.HasActiveFilters.Should().BeTrue();
            model.HasNoFilterResults.Should().BeFalse();
            model.Courses.Should().ContainSingle(course => course.LarsCode == "105");
            model.Filters.ShowFilterOptions.Should().BeTrue();
            model.Filters.ClearFilterSections.Should().ContainSingle(section => section.Title == CourseNameSectionHeading);
            model.Filters.ClearFilterSections.Single().Items.Single().DisplayText.Should().Be("Alpha");
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndDeliveryStatusFilterMatches_ThenReturnsMatchingCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        GetRestrictedApprenticeshipsResponse response,
        string providerName,
        int ukprn)
    {
        response.Courses =
        [
            new RestrictedApprenticeshipModel
            {
                LarsCode = "200",
                Title = "Closed course",
                Level = 3,
                LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
                IsClosedToNewStarts = true
            },
            new RestrictedApprenticeshipModel
            {
                LarsCode = "105",
                Title = "Last start course",
                Level = 6,
                LastDateStarts = DateTime.UtcNow.Date.AddDays(5),
                IsClosedToNewStarts = false
            }
        ];

        SetupController(sut, outerApiClientMock, ukprn, providerName, response);

        var result = await sut.Index(
            ukprn,
            new GetRestrictedApprenticeshipsRequest
            {
                DeliveryStatus = [DeliveryStatus.LastStartDateAdded]
            },
            CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            model!.Courses.Should().ContainSingle(course => course.LarsCode == "105");
            model.Filters.ClearFilterSections.Should().ContainSingle(section => section.Title == DeliveryStatusSectionHeading);
            model.Filters.FilterSections.OfType<CheckboxListFilterSectionViewModel>()
                .Single().Items.Should().HaveCount(2);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndFiltersHaveNoMatches_ThenShowsNoFilterResults(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        GetRestrictedApprenticeshipsResponse response,
        string providerName,
        int ukprn)
    {
        response.Courses =
        [
            new RestrictedApprenticeshipModel
            {
                LarsCode = "105",
                Title = "Alpha course",
                Level = 6,
                LastDateStarts = DateTime.UtcNow.Date.AddDays(5),
                IsClosedToNewStarts = false
            }
        ];

        SetupController(sut, outerApiClientMock, ukprn, providerName, response);

        var result = await sut.Index(
            ukprn,
            new GetRestrictedApprenticeshipsRequest { SearchTerm = "nomatch" },
            CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            model!.HasActiveFilters.Should().BeTrue();
            model.HasCourses.Should().BeFalse();
            model.HasNoCourses.Should().BeFalse();
            model.HasNoFilterResults.Should().BeTrue();
            model.ShowCourseResults.Should().BeTrue();
            model.Filters.ShowFilterOptions.Should().BeTrue();
            model.Courses.Should().BeEmpty();
        }
    }

    private static void SetupController(
        RestrictedApprenticeshipsController sut,
        Mock<IOuterApiClient> outerApiClientMock,
        int ukprn,
        string providerName,
        GetRestrictedApprenticeshipsResponse response)
    {
        sut.AddTempData();
        sut.TempData[TempDataKeys.ProviderLegalName] = providerName;
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderSummary, ProviderSummaryUrl)
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);

        outerApiClientMock
            .Setup(c => c.GetRestrictedApprenticeships(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedApprenticeshipsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                response,
                new RefitSettings(),
                null));
    }
}

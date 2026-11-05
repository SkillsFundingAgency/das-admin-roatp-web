using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageRestrictedProvider.ProviderAllowedCoursesControllerTests;

[TestFixture]
public class ProviderAllowedCoursesControllerFilterTests
{
    private const int Ukprn = 10019900;
    private const string ProviderName = "Denton Business Services Limited";
    private const string ProviderSummaryUrl = "/providers/10019900";
    private const string AddAllowedCourseUrl = "/providers/10019900/Apprenticeship/allowed-courses/add";
    private const string AllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndCourseNameFilterMatches_ThenReturnsMatchingCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, CreateCoursesResponse());
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel { SearchTerm = "Alpha" },
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model!.HasActiveFilters.Should().BeTrue();
            model.HasNoFilteredResults.Should().BeFalse();
            model.Courses.Should().ContainSingle(course => course.LarsCode == "105");
            model.Filters.ShowFilterOptions.Should().BeTrue();
            model.Filters.ClearFilterSections.Should().ContainSingle(section => section.Title == CourseNameSectionHeading);
            model.Filters.ClearFilterSections.Single().Items.Single().DisplayText.Should().Be("Alpha");
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndDeliveryStatusFilterMatches_ThenReturnsMatchingCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, CreateCoursesResponse());
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel
            {
                DeliveryStatus = [DeliveryStatus.LastStartDateAdded]
            },
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model!.Courses.Should().ContainSingle(course => course.LarsCode == "105");
            model.Filters.ClearFilterSections.Should().ContainSingle(section => section.Title == DeliveryStatusSectionHeading);
            model.Filters.FilterSections.OfType<CheckboxListFilterSectionViewModel>()
                .Single().Items.Should().HaveCount(3);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndFiltersHaveNoMatches_ThenShowsNoFilterResults(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock, outerApiClientMock, CreateCoursesResponse());
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel { SearchTerm = "nomatch" },
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model!.HasActiveFilters.Should().BeTrue();
            model.HasCourses.Should().BeFalse();
            model.HasNoCourses.Should().BeFalse();
            model.HasNoFilteredResults.Should().BeTrue();
            model.ShowCourseResults.Should().BeTrue();
            model.Filters.ShowFilterOptions.Should().BeTrue();
            model.Courses.Should().BeEmpty();
        }
    }

    private static void SetupUrlHelper(ProviderAllowedCoursesController sut)
    {
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderSummary, ProviderSummaryUrl)
            .AddUrlForRoute(RouteNames.AddAllowedCourse, AddAllowedCourseUrl)
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, AllowedCoursesUrl)
            .AddUrlForRoute(RouteNames.ChangeAllowedCourseLastStartDate)
            .AddUrlForRoute(RouteNames.AddAllowedCourseLastStartDate);
    }

    private static void SetupRestrictedProvider(
        Mock<IUkprnService> ukprnServiceMock,
        Mock<IOuterApiClient> outerApiClientMock,
        GetAllowedCoursesResponse response)
    {
        ukprnServiceMock.SetupOrganisationRestriction(Ukprn, isRestricted: true, legalName: ProviderName);
        outerApiClientMock
            .Setup(c => c.GetAllowedCourses(Ukprn, CourseType.Apprenticeship, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create(HttpStatusCode.OK, response));
    }

    private static GetAllowedCoursesResponse CreateCoursesResponse()
        => new()
        {
            AllowedCourses =
            [
                new ProviderAllowedCourseModel
                {
                    LarsCode = "200",
                    Title = "Zebra course",
                    Level = 3,
                    LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
                    IsClosedToNewStarts = true
                },
                new ProviderAllowedCourseModel
                {
                    LarsCode = "105",
                    Title = "Alpha course",
                    Level = 6,
                    LastDateStarts = DateTime.UtcNow.Date.AddDays(5),
                    IsClosedToNewStarts = false
                },
                new ProviderAllowedCourseModel
                {
                    LarsCode = "150",
                    Title = "Bravo course",
                    Level = 2,
                    LastDateStarts = null,
                    IsClosedToNewStarts = false
                }
            ]
        };
}

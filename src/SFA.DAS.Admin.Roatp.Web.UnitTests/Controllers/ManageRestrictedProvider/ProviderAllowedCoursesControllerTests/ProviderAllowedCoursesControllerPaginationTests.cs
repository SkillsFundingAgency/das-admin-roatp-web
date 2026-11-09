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
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageRestrictedProvider.ProviderAllowedCoursesControllerTests;

[TestFixture]
public class ProviderAllowedCoursesControllerPaginationTests
{
    private const int Ukprn = 10019900;
    private const string ProviderName = "Denton Business Services Limited";
    private const string ProviderSummaryUrl = "/providers/10019900";
    private const string AddAllowedCourseUrl = "/providers/10019900/Apprenticeship/allowed-courses/add";
    private const string AllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndMoreThanTenCourses_ThenReturnsFirstPageOfTen(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupController(sut, outerApiClientMock, ukprnServiceMock, CreateCourses(15));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel(),
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model!.TotalCount.Should().Be(15);
            model.Courses.Should().HaveCount(PaginationViewModel.DefaultPageSize);
            model.Pagination.Pages.Should().Contain(p => p.Title == PaginationViewModel.NextPageTitle);
            model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.PreviousPageTitle);
            model.Pagination.Pages.Should().Contain(p =>
                p.Title == PaginationViewModel.NextPageTitle
                && p.Url!.Contains($"#{AllowedCoursesFilterBuilder.AllowedCourseFilterResultsFragment}"));
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndPageNumberIsTwo_ThenReturnsSecondPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupController(sut, outerApiClientMock, ukprnServiceMock, CreateCourses(15));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel { PageNumber = 2 },
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model!.TotalCount.Should().Be(15);
            model.Courses.Should().HaveCount(5);
            model.Pagination.PageNumber.Should().Be(2);
            model.Pagination.Pages.Should().Contain(p =>
                p.Title == PaginationViewModel.PreviousPageTitle
                && p.Url!.Contains("PageNumber=1"));
            model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.NextPageTitle);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndTenOrFewerCourses_ThenDoesNotShowPagination(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupController(sut, outerApiClientMock, ukprnServiceMock, CreateCourses(10));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel(),
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model!.Courses.Should().HaveCount(10);
            model.Pagination.Pages.Should().BeEmpty();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndPageNumberIsLessThanOne_ThenReturnsFirstPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupController(sut, outerApiClientMock, ukprnServiceMock, CreateCourses(15));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel { PageNumber = 0 },
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model!.TotalCount.Should().Be(15);
            model.Courses.Should().HaveCount(PaginationViewModel.DefaultPageSize);
            model.Pagination.PageNumber.Should().Be(1);
            model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.PreviousPageTitle);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndPageNumberExceedsTotalPages_ThenReturnsLastPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupController(sut, outerApiClientMock, ukprnServiceMock, CreateCourses(15));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel { PageNumber = 99 },
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model!.TotalCount.Should().Be(15);
            model.Courses.Should().HaveCount(5);
            model.Pagination.PageNumber.Should().Be(2);
            model.Pagination.Pages.Should().Contain(p => p.Title == PaginationViewModel.PreviousPageTitle);
            model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.NextPageTitle);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndFiltersApplied_ThenPaginationLinksPreserveFilters(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupController(sut, outerApiClientMock, ukprnServiceMock, CreateCourses(15));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel
            {
                SearchTerm = "Course",
                DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
            },
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;

        using (new AssertionScope())
        {
            model!.TotalCount.Should().Be(15);
            model.Courses.Should().HaveCount(PaginationViewModel.DefaultPageSize);
            model.Pagination.Pages.Should().Contain(p =>
                p.Title == PaginationViewModel.NextPageTitle
                && p.Url!.Contains("SearchTerm=Course")
                && p.Url.Contains($"DeliveryStatus={nameof(DeliveryStatus.ClosedToNewStarts)}")
                && p.Url.Contains("PageNumber=2"));
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndMoreThanSixPages_ThenShowsAtMostSixPageLinks(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupController(sut, outerApiClientMock, ukprnServiceMock, CreateCourses(70));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel { PageNumber = 4 },
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;
        var numberedPages = model!.Pagination.Pages
            .Where(p => p.Title != PaginationViewModel.PreviousPageTitle
                        && p.Title != PaginationViewModel.NextPageTitle)
            .ToList();

        using (new AssertionScope())
        {
            numberedPages.Should().HaveCount(6);
            numberedPages[2].Title.Should().Be("4");
            numberedPages[2].HasLink.Should().BeFalse();
            model.Pagination.Pages.Should().Contain(p => p.Title == PaginationViewModel.PreviousPageTitle);
            model.Pagination.Pages.Should().Contain(p => p.Title == PaginationViewModel.NextPageTitle);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndOnFirstPageOfMany_ThenCurrentPageIsNotCentred(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupController(sut, outerApiClientMock, ukprnServiceMock, CreateCourses(70));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel(),
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;
        var numberedPages = model!.Pagination.Pages
            .Where(p => p.Title != PaginationViewModel.PreviousPageTitle
                        && p.Title != PaginationViewModel.NextPageTitle)
            .ToList();

        using (new AssertionScope())
        {
            numberedPages.Should().HaveCount(6);
            numberedPages[0].Title.Should().Be("1");
            numberedPages[0].HasLink.Should().BeFalse();
            model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.PreviousPageTitle);
            model.Pagination.Pages.Should().Contain(p =>
                p.Title == PaginationViewModel.NextPageTitle
                && p.Url!.Contains("PageNumber=2"));
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAllowedCourses_AndOnLastPageOfMany_ThenCurrentPageIsNotCentred(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] ProviderAllowedCoursesController sut)
    {
        SetupController(sut, outerApiClientMock, ukprnServiceMock, CreateCourses(70));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new GetProviderAllowedCoursesRequestModel { PageNumber = 7 },
            new CancellationToken()) as ViewResult;
        var model = result!.Model as AllowedCoursesViewModel;
        var numberedPages = model!.Pagination.Pages
            .Where(p => p.Title != PaginationViewModel.PreviousPageTitle
                        && p.Title != PaginationViewModel.NextPageTitle)
            .ToList();

        using (new AssertionScope())
        {
            numberedPages.Should().HaveCount(6);
            numberedPages[^1].Title.Should().Be("7");
            numberedPages[^1].HasLink.Should().BeFalse();
            model.Pagination.Pages.Should().Contain(p =>
                p.Title == PaginationViewModel.PreviousPageTitle
                && p.Url!.Contains("PageNumber=6"));
            model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.NextPageTitle);
        }
    }

    private static void SetupController(
        ProviderAllowedCoursesController sut,
        Mock<IOuterApiClient> outerApiClientMock,
        Mock<IUkprnService> ukprnServiceMock,
        List<ProviderAllowedCourseModel> courses)
    {
        ukprnServiceMock.SetupOrganisationRestriction(Ukprn, isRestricted: true, legalName: ProviderName);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderSummary, ProviderSummaryUrl)
            .AddUrlForRoute(RouteNames.AddAllowedCourse, AddAllowedCourseUrl)
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, AllowedCoursesUrl)
            .AddUrlForRoute(RouteNames.ChangeAllowedCourseLastStartDate)
            .AddUrlForRoute(RouteNames.AddAllowedCourseLastStartDate);

        outerApiClientMock
            .Setup(c => c.GetAllowedCourses(Ukprn, CourseType.Apprenticeship, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create(
                HttpStatusCode.OK,
                new GetAllowedCoursesResponse { AllowedCourses = courses }));
    }

    private static List<ProviderAllowedCourseModel> CreateCourses(int count)
        => Enumerable.Range(1, count)
            .Select(i => new ProviderAllowedCourseModel
            {
                LarsCode = $"{100 + i}",
                Title = $"Course {i:D2}",
                Level = 2,
                LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
                IsClosedToNewStarts = true
            })
            .ToList();
}

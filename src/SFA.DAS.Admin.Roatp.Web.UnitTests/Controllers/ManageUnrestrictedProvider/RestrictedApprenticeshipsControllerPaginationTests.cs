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
using SFA.DAS.Admin.Roatp.Web.Models.Shared;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider;

[TestFixture]
public class RestrictedApprenticeshipsControllerPaginationTests
{
    private const string ProviderSummaryUrl = "/providers/10019900";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndMoreThanTenCourses_ThenReturnsFirstPageOfTen(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        SetupController(sut, outerApiClientMock, ukprn, providerName, CreateCourses(15));

        var result = await sut.Index(ukprn, new GetRestrictedApprenticeshipsModel(), CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            model!.TotalCount.Should().Be(15);
            model.Courses.Should().HaveCount(PaginationViewModel.DefaultPageSize);
            model.Pagination.Pages.Should().Contain(p => p.Title == PaginationViewModel.NextPageTitle);
            model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.PreviousPageTitle);
            model.Pagination.Pages.Should().Contain(p =>
                p.Title == PaginationViewModel.NextPageTitle
                && p.Url!.Contains($"#{RestrictedApprenticeshipsFilterBuilder.RestrictedApprenticeshipFilterResultsFragment}"));
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndPageNumberIsTwo_ThenReturnsSecondPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        SetupController(sut, outerApiClientMock, ukprn, providerName, CreateCourses(15));

        var result = await sut.Index(
            ukprn,
            new GetRestrictedApprenticeshipsModel { PageNumber = 2 },
            CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;

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
    public async Task WhenGettingRestrictedApprenticeships_AndTenOrFewerCourses_ThenDoesNotShowPagination(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        SetupController(sut, outerApiClientMock, ukprn, providerName, CreateCourses(10));

        var result = await sut.Index(ukprn, new GetRestrictedApprenticeshipsModel(), CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            model!.Courses.Should().HaveCount(10);
            model.Pagination.Pages.Should().BeEmpty();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndPageNumberIsLessThanOne_ThenReturnsFirstPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        SetupController(sut, outerApiClientMock, ukprn, providerName, CreateCourses(15));

        var result = await sut.Index(
            ukprn,
            new GetRestrictedApprenticeshipsModel { PageNumber = 0 },
            CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            model!.TotalCount.Should().Be(15);
            model.Courses.Should().HaveCount(PaginationViewModel.DefaultPageSize);
            model.Pagination.PageNumber.Should().Be(1);
            model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.PreviousPageTitle);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndPageNumberExceedsTotalPages_ThenReturnsLastPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        SetupController(sut, outerApiClientMock, ukprn, providerName, CreateCourses(15));

        var result = await sut.Index(
            ukprn,
            new GetRestrictedApprenticeshipsModel { PageNumber = 99 },
            CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;

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
    public async Task WhenGettingRestrictedApprenticeships_AndFiltersApplied_ThenPaginationLinksPreserveFilters(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        SetupController(sut, outerApiClientMock, ukprn, providerName, CreateCourses(15));

        var result = await sut.Index(
            ukprn,
            new GetRestrictedApprenticeshipsModel
            {
                SearchTerm = "Course",
                DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
            },
            CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;

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
    public async Task WhenGettingRestrictedApprenticeships_AndMoreThanSixPages_ThenShowsAtMostSixPageLinks(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        SetupController(sut, outerApiClientMock, ukprn, providerName, CreateCourses(70));

        var result = await sut.Index(
            ukprn,
            new GetRestrictedApprenticeshipsModel { PageNumber = 4 },
            CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;
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
    public async Task WhenGettingRestrictedApprenticeships_AndOnFirstPageOfMany_ThenCurrentPageIsNotCentred(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        SetupController(sut, outerApiClientMock, ukprn, providerName, CreateCourses(70));

        var result = await sut.Index(ukprn, new GetRestrictedApprenticeshipsModel(), CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;
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
    public async Task WhenGettingRestrictedApprenticeships_AndOnLastPageOfMany_ThenCurrentPageIsNotCentred(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        SetupController(sut, outerApiClientMock, ukprn, providerName, CreateCourses(70));

        var result = await sut.Index(
            ukprn,
            new GetRestrictedApprenticeshipsModel { PageNumber = 7 },
            CancellationToken.None) as ViewResult;
        var model = result!.Model as RestrictedApprenticeshipsViewModel;
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
        RestrictedApprenticeshipsController sut,
        Mock<IOuterApiClient> outerApiClientMock,
        int ukprn,
        string providerName,
        List<RestrictedApprenticeshipModel> courses)
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
                new GetRestrictedApprenticeshipsResponse { Courses = courses },
                new RefitSettings(),
                null));
    }

    private static List<RestrictedApprenticeshipModel> CreateCourses(int count)
        => Enumerable.Range(1, count)
            .Select(i => new RestrictedApprenticeshipModel
            {
                LarsCode = $"{100 + i}",
                Title = $"Course {i:D2}",
                Level = 2,
                LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
                IsClosedToNewStarts = true
            })
            .ToList();
}

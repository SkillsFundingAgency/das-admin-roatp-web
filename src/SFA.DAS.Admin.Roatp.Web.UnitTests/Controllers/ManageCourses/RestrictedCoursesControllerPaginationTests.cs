using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class RestrictedCoursesControllerPaginationTests
{
    private const string RestrictedCoursesUrl = "/restricted-courses";

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourses_AndMoreThanTenCourses_ThenReturnsFirstPageOfTen(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCoursesController sut)
    {
        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRestrictedCoursesResponse { Courses = CreateCourses(15) });

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(new GetRestrictedCoursesRequest(), CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCoursesViewModel;
        model!.TotalCount.Should().Be(15);
        model.Courses.Should().HaveCount(PaginationViewModel.DefaultPageSize);
        model.Pagination.Pages.Should().Contain(p => p.Title == PaginationViewModel.NextPageTitle);
        model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.PreviousPageTitle);
        model.Pagination.Pages.Should().Contain(p =>
            p.Title == PaginationViewModel.NextPageTitle
            && p.Url!.Contains($"#{RestrictedCoursesFilterBuilder.RestrictedCourseFilterResultsFragment}"));
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourses_AndPageNumberIsTwo_ThenReturnsSecondPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCoursesController sut)
    {
        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRestrictedCoursesResponse { Courses = CreateCourses(15) });

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(
            new GetRestrictedCoursesRequest { PageNumber = 2 },
            CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCoursesViewModel;
        model!.TotalCount.Should().Be(15);
        model.Courses.Should().HaveCount(5);
        model.Pagination.Pages.Should().Contain(p => p.Title == PaginationViewModel.PreviousPageTitle);
        model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.NextPageTitle);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourses_AndTenOrFewerCourses_ThenDoesNotShowPagination(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCoursesController sut)
    {
        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRestrictedCoursesResponse { Courses = CreateCourses(10) });

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(new GetRestrictedCoursesRequest(), CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCoursesViewModel;
        model!.Courses.Should().HaveCount(10);
        model.Pagination.Pages.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourses_AndPageNumberIsLessThanOne_ThenReturnsFirstPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCoursesController sut)
    {
        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRestrictedCoursesResponse { Courses = CreateCourses(15) });

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(
            new GetRestrictedCoursesRequest { PageNumber = 0 },
            CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCoursesViewModel;
        model!.TotalCount.Should().Be(15);
        model.Courses.Should().HaveCount(PaginationViewModel.DefaultPageSize);
        model.Pagination.PageNumber.Should().Be(1);
        model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.PreviousPageTitle);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourses_AndPageNumberExceedsTotalPages_ThenReturnsLastPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCoursesController sut)
    {
        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRestrictedCoursesResponse { Courses = CreateCourses(15) });

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, RestrictedCoursesUrl);

        var result = await sut.Index(
            new GetRestrictedCoursesRequest { PageNumber = 99 },
            CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCoursesViewModel;
        model!.TotalCount.Should().Be(15);
        model.Courses.Should().HaveCount(5);
        model.Pagination.PageNumber.Should().Be(2);
        model.Pagination.Pages.Should().Contain(p => p.Title == PaginationViewModel.PreviousPageTitle);
        model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.NextPageTitle);
    }

    private static List<RestrictedCourseModel> CreateCourses(int count)
        => Enumerable.Range(1, count)
            .Select(i => new RestrictedCourseModel
            {
                LarsCode = $"{100 + i}",
                Title = $"Course {i:D2}",
                Level = 2,
                LearningType = LearningType.Apprenticeship
            })
            .ToList();
}

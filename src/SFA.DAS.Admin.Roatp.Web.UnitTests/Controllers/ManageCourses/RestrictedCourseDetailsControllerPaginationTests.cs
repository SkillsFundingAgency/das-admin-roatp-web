using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
public class RestrictedCourseDetailsControllerPaginationTests
{
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = "/restricted-courses/" + LarsCode;
    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndMoreThanTenProviders_ThenReturnsFirstPageOfTen(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;
        response.Providers = CreateProviders(15);

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.ProviderCount.Should().Be(15);
        model.AllowedProviders.Should().HaveCount(PaginationViewModel.DefaultPageSize);
        model.Pagination.Pages.Should().Contain(p => p.Title == PaginationViewModel.NextPageTitle);
        model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.PreviousPageTitle);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndPageNumberIsTwo_ThenReturnsSecondPage(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;
        response.Providers = CreateProviders(15);

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var request = new GetRestrictedCourseDetailsRequest { PageNumber = 2 };

        var result = await sut.Index(LarsCode, request, CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.ProviderCount.Should().Be(15);
        model.AllowedProviders.Should().HaveCount(5);
        model.Pagination.Pages.Should().Contain(p => p.Title == PaginationViewModel.PreviousPageTitle);
        model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.NextPageTitle);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndTenOrFewerProviders_ThenDoesNotShowPagination(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;
        response.Providers = CreateProviders(10);

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.AllowedProviders.Should().HaveCount(10);
        model.Pagination.Pages.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndPageNumberIsLessThanOne_ThenReturnsFirstPage(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;
        response.Providers = CreateProviders(15);

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var request = new GetRestrictedCourseDetailsRequest { PageNumber = 0 };

        var result = await sut.Index(LarsCode, request, CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.ProviderCount.Should().Be(15);
        model.AllowedProviders.Should().HaveCount(PaginationViewModel.DefaultPageSize);
        model.Pagination.PageNumber.Should().Be(1);
        model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.PreviousPageTitle);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndPageNumberExceedsTotalPages_ThenReturnsLastPage(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] RestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;
        response.Providers = CreateProviders(15);

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var request = new GetRestrictedCourseDetailsRequest { PageNumber = 99 };

        var result = await sut.Index(LarsCode, request, CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.ProviderCount.Should().Be(15);
        model.AllowedProviders.Should().HaveCount(5);
        model.Pagination.PageNumber.Should().Be(2);
        model.Pagination.Pages.Should().Contain(p => p.Title == PaginationViewModel.PreviousPageTitle);
        model.Pagination.Pages.Should().NotContain(p => p.Title == PaginationViewModel.NextPageTitle);
    }

    private static List<ProviderCourseModel> CreateProviders(int count)
        => Enumerable.Range(1, count)
            .Select(i => new ProviderCourseModel
            {
                Ukprn = 10000000 + i,
                ProviderName = $"Provider {i:D2}",
                LastDateStarts = null
            })
            .ToList();
}
